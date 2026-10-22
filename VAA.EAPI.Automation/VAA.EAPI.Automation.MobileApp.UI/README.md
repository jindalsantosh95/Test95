# Mobile App UI Automation

**PayPal TC57:** explicit local SIT login readiness, disabled by default and excluded from
release packs. Reuses Core Vault and web provider selectors; approves once and requires
confirmation and Trips. Known confirmation errors fail; separate payment opt-in required.
Read [privacy prerequisites and remaining coverage](PAYPAL-COVERAGE.md) before enabling.

**Review figure retention:** TC56 now retains the first Review breakdown and compares
Flights, taxes/fees and Total individually on re-entry, including currency and points.
The reusable assertion also compares optional seat-row presence and amount. Both visits
still reconcile arithmetic and the pre-checkout Summary total. This catches offsetting
fare/tax changes that leave the total unchanged; it is UI retention evidence, not an
independent backend tax calculation. The earlier local TC56 pass predates this extension.

**Review functional follow-up:** TC56 prepares no-payment back/re-entry validation;
run explicitly until qualified. Fare-condition/itinerary navigation needs additional owned
app IDs. Visual/accessibility checks are manually owned and PWP is outside this MVP ticket.
See [current scope and blockers](REVIEW-TOTAL-COVERAGE.md).

**Pre-HPP Review total:** shared complete-booking checkout now checks the price breakdown,
retained Summary total/currency/reward points, guest PWP exclusion and guidance/actions.
See [coverage and remaining gaps](REVIEW-TOTAL-COVERAGE.md). Offline implementation only;
live requalification is required. Existing pack counts are unchanged.

**TC53 route correction — 20 October:** MAN–MIA one-way, one adult and one child,
Air France-led Economy Standard. Every operating leg must be explicitly unsupported
for seats; the connection is not fixed. This avoids using a Light fare to mask the
carrier-based bypass. Logs cabin/fare and stops on payment without paying.
Local retest passed: Air France Economy Standard selected, every leg verified unsupported,
then payment reached without guidance or seat actions. Test 3m25s / total 3m35s.
Evidence: `TestResults/tc53-man-mia-live-20261020/tc53.trx`; analyser found no failures.
All 548 offline checks and scoped harness passed.

**TC52 fare correction — 20 October:** require Virgin Economy Classic inbound, excluding
Economy Light because it bypasses seats. Connections remain dynamic and every outbound
leg must still be unsupported. Both selected cabin/fare names are written to test output.
No Delta fallback until its seat-eligible fare contract is qualified.
Local retest **passed**: KLM Economy Light outbound (all legs verified unsupported) and
Virgin Economy Classic inbound. Child proximity guidance displayed. No seat submission
or payment. Test duration 2m45s, total run 3m01s; 548 offline checks and scoped harness passed.
TRX/analyser: `TestResults/tc52-classic-live-20261020/tc52.trx`, no failures.
This supersedes the unrestricted inbound fare selection described below.

**TC52 selection correction — 20 October:** the current scenario dynamically selects an
outbound whose every leg is unsupported for seats, then an inbound containing a verified
Virgin or Delta-operated leg. The fixed JFK connection, leg count, AF-only preference and
fare-name requirements below are superseded. The assertion checks guidance on entering
the first eligible inbound sector, not the separate warning for seats chosen too far apart.
Local retest selected both flights and completed passenger details, but the app opened
payment directly instead of seat selection/guidance. Test failed explicitly at that boundary;
no payment was attempted. Cause is unconfirmed (eligibility/configuration/app handling).
Evidence: `TestResults/tc52-dynamic-live-20261020/tc52.trx` (2m53s total).
All 548 offline checks and the scoped harness passed.

**TC52 route update — 20 October:** use YYZ–LHR return for one adult and one child.
Require an Air France-led outbound with both legs explicitly AF/KLM (not necessarily both AF)
and Virgin LHR–JFK / Delta JFK–YYZ inbound,
verified from Flight Details before fare selection. This preserves the first-eligible-sector
guidance check (flight 3 of 4). No fallback route or retry after a service error is used.
The earlier LHR–JFK run failed on the partner fare sheet before guidance; this route change
does not resolve that downstream issue. The revised local run failed after 2m07s while
locating `NativeFlightResult4FlightDetails`, before outbound fare selection/proximity.
Flight-details lookup now uses bounded small scrolls and visible result IDs to recover an
overshot candidate instead of continuing down the list. Unknown carriers remain rejected.
Retest reached and selected the outbound, confirming recovery past this failure.
It then failed return selection after 6m49s total: no Virgin cabin was attempted after
candidate checks. Inventory versus selection cause is unconfirmed; guidance was not reached.
Current evidence: `TestResults/tc52-scroll-live-20261020/tc52.trx`.
All 551 offline checks and the scoped harness passed. Previous evidence:
`TestResults/tc52-yyz-live-20261020/tc52.trx`. All 547 offline checks and the scoped harness passed.

**Full-booking release coverage — 19 October:** Sanity now contains 14 scenarios and
Regression 53. TC23 now completes a one-adult JFK–LHR Virgin Economy Classic booking,
checking USD on results, Summary and confirmation, plus the booking reference in Trips.
TC43 requires seat selection and completes payment, confirmation and Trips; it is now in
both packs. Loyalty TC01 already completes logged-in Economy redemption using a leased
high-balance account. Together with TC48, these are four complete booking paths in Sanity.
This is implementation/enrolment, not new live passing evidence: TC23's expanded checkout
and TC43's historical payment issue need requalification. TC43 does not yet assert exact
post-payment seat fulfilment per traveller/sector. Older dated notes below are historical.

**Post-HPP error contract — 19 October:** FAULT-TC07–09 now expect the dedicated
non-retryable page introduced by iOS PR1329 (develop `de727a5e`). Checks require its
owned page/title/message/action IDs, exact heading and contact-centre message, enabled
“Return to flight search”, no payment Retry and no confirmation. They observe the action
without tapping it or resubmitting payment. TC07 passed locally; TC08/09 and BrowserStack
qualification remain pending. This does not
qualify the full Review Page story, dark mode, telephone interaction or fresh-search reset.

Local TC07 passed in 2m49s against iOS PR1331 (`0dd5d294`) on 19 October, after its
accessibility-only fix exposed the owned Return button ID. The status-update fault was
intercepted and the exact page/title/message/action contract passed without retry or
confirmation. Exact native-label checks avoid duplicate label/value/text. All 539 offline
and retry-policy checks also passed. Keep the scenarios Draft and separately opted in.
Evidence: `TestResults/payment-error-pr1331-tc07-20261019/tc07.trx` and
`TestResults/payment-error-pr1331-offline-20261019/offline.trx` (local generated files).

**Confirmation warning preparation — 16 October:** six code-driven fixture cases cover
W001/W002/W003, the two valid combined warnings and a no-warning control. Each warning includes
the exact backend message; UI checks must display all returned messages without enforcing order. These remain
Draft preparation, not executable UI tests or release coverage. See the
[activation checklist](../docs/MOBILE_FAULT_INJECTION.md#prepared-confirmation-warnings--16-october-2026)
for app-contract and guarded success-response proxy dependencies.

**Controlled failures — 14 October:** eleven opt-in Draft cases now exist. TC01–05 passed the
six-case BrowserStack qualification; TC06 passed locally on the provisioned simulator (3m14s)
but remains unqualified remotely after its timeout/error-control failure and Appium connection errors.
New TC07–09 target post-HPP payment-status/order errors
and order timeout; at that point they had not been run live (see the 19 October TC07 update above). They reuse test HPP payment as setup only,
require `FaultInjection.AllowPostHppTestPayment=true` in addition to the routing opt-ins,
and never click payment retry. They are excluded from normal Sanity/Regression.
New TC10/11 cover seat-map loading and seat-save failures with seats enabled. They use
existing seat error/recovery IDs and require the expected error without payment navigation;
TC11 also checks the selected draft assignment remains visible. Neither retries nor pays.
Implemented and offline-checked only; the evolving error UI still needs live qualification.
See [fault coverage and safety prerequisites](../docs/MOBILE_FAULT_INJECTION.md#latest-qualification-and-post-hpp-extension--14-october).

**Payment failure detection — 14 October:** payment preparation, HPP opening and confirmation
waits share a terminal-error check on each poll. A visible `NativeFlightPaymentRetryButton`,
or the inline text “We could not complete your booking” / “Try secure payment again”, fails
without waiting out the confirmation timeout or retrying payment. The previous confirmation
check matched buttons only and missed the static-text error in the redemption run.
The exception records the recognised error and journey stage; the existing failure handler
collects configured evidence before teardown. Screenshot/network capture remains opt-in;
this does not enable sensitive capture or prove a backend root cause.
The English text fallback is isolated in `NativePaymentFailure` and limited to text/button
controls. App contract follow-up: expose an owned ID on the inline payment error, then replace
this fallback and add that ID to the approved error-crop policy. Offline checks do not qualify
the new detector on a live device; no booking rerun is implied.

**20 October extension:** the shared detector also fails immediately on any visible
`NativeFlightPaymentErrorTitle`, `NativeFlightPaymentErrorMessage` or
`NativeFlightPaymentErrorPage`. It handles accessible children even when the page container
is not exposed. The exception identifies the stage/ID without copying sensitive message
content, and explicitly leaves payment-capture status and backend cause unconfirmed.
No retry or Return to search is clicked. Controlled-error tests retain their separate
expected-error assertions. This addition is offline-qualified only.

**Remote Flight details and result reporting — 13 October:** carrier validation uses
`NativeFlightResult{index}FlightDetails` and the `NativeFlightDetails` sheet ID, never
English-label or card-hierarchy fallback. Scrolling is bounded; missing app-owned IDs
are reported as contract failures, not bypassed. Only cards without carrier evidence
require opening Flight details.

Session status is sent independently of session naming. If the executor rejects status,
one metadata-only REST update is attempted after teardown with a ten-second timeout,
fixed BrowserStack HTTPS endpoint and redirects disabled. No app actions or bookings
are retried. Credentials/response bodies are not logged; if both paths fail, the TRX
remains authoritative and reports that remote status is unavailable. See BrowserStack's
[session API](https://www.browserstack.com/docs/app-automate/api-reference/appium/sessions).

**BrowserStack calendar settling — 13 October:** remote date-stability observations can
take 3–4 seconds each, so a three-second wait can expire before the required second
observation. Settling now uses the existing 30-second screen budget, returning as soon
as two safe matching observations succeed. It still requires a visible, enabled cell
fully inside the unobscured viewport; stale recycled cells reset the observations within
the same budget. Duplicate visible IDs also reset stability until exactly one remains.
Immediately before each intentional date tap, the selection is compared with its
pre-navigation baseline to reject delayed accidental selection updates after scrolling.
There are no fixed sleeps or extra date taps. Timeout diagnostics name
the calendar date and failed stability condition. Remote qualification is required before
claiming this resolves full-pack stability.

**Calendar local investigation — 13 October:** the original hold-and-drag reproduced the
BrowserStack failure on the iPhone 17 Pro/iOS 26.5 simulator: an empty departure became
8 November during the scroll at X=16, Y=591 to 252. The installed WebDriverAgent maps
`dragFromToForDuration` to `pressForDuration:thenDragToCoordinate:`; its duration is the
initial hold, not travel time. Calendar scrolling now uses a continuous W3C touch swipe
with 350ms movement and no pre-movement pause, retaining measured gutter/viewport bounds
and the unchanged-selection guard. This is automation-only; no iOS code changes.

The first changed local run passed all calendar assertions and reached Summary after both
reward-fare selections. Loyalty TC02 still failed waiting for `NativeFlightSummaryInsufficientPoints`,
the separately documented app-contract issue below. All 464 offline/retry checks passed.
Evidence: `TestResults/calendar-local-swipe-20261013/local.trx` under the Automation solution.
TC36 now exercises three-month-forward selection and a one-month-earlier edit, plus cancellation.
Its first no-hold run exposed overshoot oscillation; direction reversals now halve the swipe
distance, down to one eighth, within the unchanged attempt limit. The corrected TC36 passed
twice consecutively: `TestResults/calendar-local-edit-20261013-b/local.trx` (470 offline/retry
checks plus TC36, 1m46s total) and `calendar-local-edit-20261013-c/local.trx` (TC36, 1m37s).
Exact-TRX analysis, targeted formatting and project-scoped harness passed. Existing readiness
traits/pack membership are unchanged; Android and BrowserStack qualification remain outstanding.

**Calendar hardening — 13 October:** isolated BrowserStack Loyalty TC02 reproduced the same
range-settling timeout as Sanity. Range setup now navigates departure-first, never pre-visits the
return month or selects an earlier anchor date. At most two requested-departure taps implement
the app's existing pending/completed range transitions, each requiring observed state change;
final departure/return assertions remain mandatory. Scroll gestures derive their vertical span
from the calendar viewport/footer and their horizontal gutter from observed day-cell bounds.
Missing safe geometry fails without a drag; a changed selection after scrolling fails before
another tap. Date taps require an enabled, fully contained cell with stable bounds. No sleeps,
timeout increases, coordinate taps or iOS changes. Offline validation is not BrowserStack
qualification; rerun isolated Loyalty TC02 on the patched branch before claiming resolution.
Release build and 462 offline/retry checks passed, including 12 calendar geometry cases.
The exact-TRX analyser found no failures:
`TestResults/calendar-safety-final-20261013/offline.trx` under the Automation solution.

**13 October develop compatibility:** see the [source impact review](IOS-DEVELOP-REVIEW-20261013.md).
Shared checkout now validates the new confirmation's exact traveller count and names (guest input
or leased member identity), without emitting names on failure. These assertions require the new
confirmation build and live qualification. Loyalty TC02 remains unchanged: develop #1277 removed
the warning/Continue guard it expects; product clarification is required, not automatic skipping.
Pack membership is unchanged. Historical results below do not qualify the latest iOS build.

For release sign-off use [Search and Book release packs](RELEASE-PACKS.md):
`Suite=Sanity` (14) or `Suite=Regression` (53 including TC41/42), always scoped by
`RegressionArea=SearchAndBook&Platform=iOS`. Regression requires backend validation enabled
and API connectivity from the runner; the workflow configures this automatically.
The guide contains exact filters, membership, prerequisites and mandatory exclusion review.
TC44–50 are now enrolled in Regression; TC43 is now included, with its historical payment issue requiring requalification. This is pack selection,
not a claim of fresh full-suite/BrowserStack qualification. Later dated notes retain historical status.

For the current handover, use the [current testing status](TEST-CATALOG.md#current-status--12-october-2026).
The dated investigation notes below are historical and must not override that table.

HPP native input hardening (9 October): the failure screenshot showed Town / City text appended
to Address line 1 while the keyboard covered the next control. Native HPP field discovery now
dismisses an observed keyboard and waits for it to disappear, then requires the whole enabled
target to fit the screen. Billing value verification re-queries its locator for up to five seconds
after one entry attempt; it does not immediately retype or print field contents on failure.
Address line 1, Town / City and Postcode are checked again before the single Pay action.
This is automation-only, with existing bounded scrolling; no HPP/app change or payment retry.

Trips booking-reference verification accepts the compact reference and the character-spaced
VoiceOver representation used by iOS `BookingCardInfoHeader`, with exact character order and
token boundaries. It does not infer success from the destination, screenshot or a partial reference.

TC48 is the one-adult no-seats booking comparison: Virgin LHR-JFK, three months ahead,
Economy Classic, guest passenger entry, test payment, confirmation and Trips. It reuses
the existing checkout flow and explicitly enables the skip-seat launch flag for its own
session. It passed locally on main on 12 October (4m11s) and now belongs to Sanity and
Regression; TC01's five-passenger coverage is unchanged. Run with `FullyQualifiedName~SearchBook_TC48_`.
TC43 uses the same route/party/fare criteria with seat selection, but independent searches
do not guarantee identical live inventory or isolate a backend root cause.

Seats: see [the iOS code-to-test coverage matrix and required accessibility contract](SEATS-COVERAGE.md).
**Newest profiling result:** TC47 passed locally in 3m32s including lifecycle, versus 5m59s before.
Discovery-only XML omits expensive visibility/accessibility attributes; fresh live tap-safety checks
remain mandatory. 369 offline checks passed. See the seats document for timings and evidence;
this single comparison does not qualify the full pack or BrowserStack.
The 6m02s pass below belongs to the preceding implementation.
**Latest TC47: local pass, 7 October, 6m02s.** Automation-only safe-area refinement with the
rebuilt PR1273 viewport verified GBP 40 selection, GBP 35 replacement and removal back to zero.
Evidence: `TestResults/seats-tc47-safe-area-local-20261007/local.trx`. No payment; still Qualification.
This supersedes the historical missing-ID/safety-dependency notes below for TC47.
**Current safety dependency:** hardened shared seat interactions require `seatSelection.viewport`
on the app's vertical native scroll viewport. This iOS ID remains outstanding; TC43–TC47 must be
requalified after it is added. No fallback tapping or scrolling is performed when it is absent.
TC47 adds paid-seat charge/total checks for selection, differently priced replacement and removal.
The 7 October local attempt stopped at the missing `seatSelection.seatPrice` ID (2m44s).
The installed app predates PR1273 commit `c12c922d`; pull/rebuild/install before retrying.
Pricing assertions remain unqualified. It stops before submission/payment and is not regression.
TC46 adds two-adult seat assignment independence and automatic passenger advance. It passed the
7 October local rerun in 2m37s after the first run encountered a seat-map error. It uses a separate
qualification class and stops before payment; BrowserStack remains unqualified.
TC44/TC45 now isolate seat assignment lifecycle and details/key cancellation before payment.
Run `Journey=Seats&TestType=FunctionalValidation` with run-scoped TRX. They remain in
`Suite=Qualification` and passed locally on 7 October (1m57s/2m15s); BrowserStack is pending.
They report seat-stage durations; TC43 remains
the separate full booking journey. One happy-path booking is not full seat-selection coverage.

**Seats qualification (TC43):** run the independent one-adult guest seat journey with
`850100-search-native-entry` enabled and `850101-search-native-skip-seat-selection` omitted.
Persisted developer overrides must also leave skip disabled. The test requires the seat screen,
selects a visible enabled offered seat using `seatSelection.seat.*`, confirms it and verifies
the passenger assignment before submitting once. Errors fail without retry/skip. It then uses
the existing HPP, confirmation and Trips flow. Initially `Suite=Qualification`, not regression;
multi-sector and post-payment seat fulfilment are outside this first check.

The scenario requires **Economy Classic**, not the first Economy fare: iOS develop excludes
`ECONOMY-LIGHT`/`ECOLIGHT` from seat-map eligibility and permits VS/DL operated sectors.
An omitted launch flag does not clear an existing saved developer override. Launch flags only
enable requested features; for local qualification ensure the saved seat-skip toggle is off.

**Payment entry — 6 October 2026:** uses the current app-owned
`NativeFlightPaymentHeading` and `NativeFlightContinueToPaymentButton` IDs. Waits for an enabled
Continue before opening HPP; a visible `NativeFlightPaymentRetryButton` fails immediately without
retrying preparation or submitting payment. No fixed sleep or app-code change.

Latest rerun passed through test payment, confirmation and booking visibility in Trips (3m33s).
Evidence: `TestResults/guest-payment-entry-smoke-20261006/local.trx`; 338 offline checks passed.
The temporary one-adult TC01 variant was restored; this is not five-passenger qualification.
This supersedes the payment-entry failure recorded below.

**Passenger gender menu — 6 October 2026:** current iOS develop uses a native Menu.
The page object opens the passenger-specific gender ID, selects exactly one visible/enabled
native button with the requested complete label, and verifies the visible field's bound value
after dismissal. It no longer waits for or toggles a wheel; DOB remains a separate wheel control.
Menu option IDs are missing in the app. This English-label exception is confined to the gender
page method; replace it with app-owned option IDs when iOS exposes them. No coordinate fallback.
Missing or duplicate matches fail within the existing bounded wait.

Local qualification: 337 offline checks passed. The one-adult guest smoke on 6 October passed
gender selection and passenger submission, then stopped because the payment-page wait did not
recognise the visible `NativeFlightContinueToPaymentButton` state. No booking was completed.
Evidence: `TestResults/guest-gender-menu-smoke-20261006/local.trx` under the Automation solution.
Scoped formatting passes; the wider harness remains blocked by its unrelated stale
context-routing review metadata (33 days against a 30-day limit).

**TC41/TC42 UI/backend regression:** [setup and external-runner requirements](BACKEND-UI-VALIDATION.md).
Backend search and summary preparation precede a normal UI search, matching the same flight
and exact fare before comparing baggage, including dimensions. No app metadata hook.
Both are included in Regression and retain `Suite=BackendRegression&RegressionArea=SearchAndBook`
as a diagnostic subset filter. Local runs require `BackendValidation.Enabled=true`.
Only this separate backend pack requires `BackendValidation:Enabled=true` and runner backend access.
The default `Suite=Regression` pack excludes TC41/TC42 and works with backend validation disabled.
TC42 covers connecting partners. BrowserStack qualification remains pending; TC29 is unchanged.

**TC29 baggage qualification — 30 September 2026:** passed locally in 58s (1m07s run).
Economy Light retained its fare title; two inclusion/exclusion comparisons and one advertised
weight comparison passed. No quantity was advertised for cross-screen comparison. One Summary
row displayed three-axis dimensions, but no selected fare benefit advertised dimensions, so
dimension retention was **NOT EXERCISED**. TC29 now emits these counts after successful assertions;
a generic pass must not be reported as a dimension-comparison pass. 299 offline/retry tests and
scoped harness verification passed; exact-TRX analysis found no failures. Evidence:
`TestResults/tc29-baggage-local-20260930/local.trx`. No checkout/payment or iOS changes.
To finish dimension accuracy coverage, use a dimension-bearing fare or correlate the selected
app offer/basket with authoritative backend baggage data. A separate API search is not proof
of this app selection. Linear dimensions and full fare rules remain outside this assertion.

**TC26 per-leg extension passed locally — 30 September 2026.** The rebuilt app exposes the
new scoped IDs. Results and Summary Flight details retained ordered leg airports, full dates,
clock times, operating carriers and supplied descriptors; adjacent/connection airports and
search endpoints/date matched. Existing stop-count and +1 assertions also passed. One scenario
passed in 2m17s (2m32s including session overhead), with no checkout/payment. Exact-TRX analysis
found no failures: `TestResults/tc26-legs-local-20260930-rebuilt/local.trx`.
This supersedes the installed-build blocker below. The pack remains 41 scenarios, 38 with passing
evidence, two on hold and one skipped; this was not a full-pack, Android or BrowserStack run.

**TC26 per-leg extension — local run blocked, 30 September 2026.**
The extended scenario reached Flight details but could not find
`NativeFlightDetailsSegment1OriginAirportCode` after bounded scrolling. The installed simulator
binary lacked `DestinationAirportCode`, `DetailedFlightRowAccessibility` and
`FlightRowFieldAccessibility` markers, consistent with rebuilding a different checkout.
Build/install from the isolated iOS `feature/search-book-leg-accessibility` workspace before
retrying. Do not add label/coordinate fallbacks. The 297 offline/retry checks pass, including
14 new evidence/continuity/retention cases; exact-TRX analysis completed for both runs.
Live evidence: `TestResults/tc26-legs-local-20260930/local.trx` (one failure, 1m54s scenario;
2m11s test-run duration). No checkout/payment. Historical TC26 passes below do not qualify
the new per-leg assertions. No full-pack, Android or BrowserStack run was made.

**TC40 return Summary Change qualified locally — 30 September 2026.**
`SearchBook_TC40_ReturnSummaryChange_RequiresInboundReselectionAndUpdatesItinerariesAndTotal`
passed in 2m54s on iPhone 17 Pro/iOS 26.5. It changes the outbound, requires inbound reselection,
then validates both replacement itineraries, exact fares and total (including signed inbound
price adjustments). No checkout/payment. Reuses existing app-owned IDs; one independent class
for remote parallel execution. Inbound results deliberately have no flexible-date carousel;
verify their date on the scoped Summary itinerary, not using the outbound carousel helper.
283 offline/retry checks and the live scenario passed; Release build, exact-TRX analysis and
scoped harness verification passed. Evidence:
`TestResults/tc40-local-20260930-inboundcontract/local.trx`.
Current consolidated evidence: **41 scenarios, 38 with passing evidence, two on hold, one skipped**.
No full-pack rerun, Android or BrowserStack qualification. Earlier totals below are historical.

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

Second gap-filling pass: TC33 covers fare-sheet cancellation, TC34 the missing passenger-name
guard, and TC35 the Terms gate for a fully populated single guest. These stop before payment
and passed locally on 29 September 2026. The shared checkout flow now exposes a
pre-Terms preparation boundary that reuses its existing synthetic traveller/contact entry; normal
guest E2Es retain the same entry order, Terms acceptance and checkout behaviour. Regression now
contains 37 scenarios with 34 consolidated passing results, two unresolved/on hold and one skipped.
TC31-TC35 passed together on iPhone 17 Pro/iOS 26.5 in 5m49s; exact-TRX analysis found no failures
(`TestResults/gap-scenarios-local-20260929/local.trx`). No payment was attempted.
This qualifies one local run, not BrowserStack execution or long-term stability.
See the gap register for the exact missing Summary Change/Upgrade and member-menu action IDs.

Coverage update (29 September 2026): see [the gap register](SEARCH-BOOK-COVERAGE-GAPS.md) for
the focused iOS source review and remaining behavioural gaps. TC31/TC32 add panel-only discount
validation, trimming, apply/cancel/replacement and clear/reset. They use existing app-owned IDs,
synthetic non-submitted codes and separate classes; no new library, backend client or iOS edit.
They are included in regression and passed locally on 29 September 2026. No discount entitlement
or performance improvement is claimed. The latest redemption TC01 passed through confirmation
and Trips on 29 September; earlier failure summaries below are historical.

Latest requalification (28 September 2026, after the user rebuilt develop with PR #1218):
the one-adult guest journey passed gender selection/retention, DOB, contact details, passenger
save and Terms, and proceeded through HPP. It then failed because the app displayed
`Try secure payment again`; confirmation and Trips were not reached. The exact payment/API
cause is not available from this local native session. No automatic payment retry was made.
The run passed all 139 offline/retry checks and failed its single E2E. This supersedes the
passenger accessibility blocker recorded below; those earlier findings describe the old build.
The temporary one-adult TC01 source was restored; this is not mixed-passenger qualification.

Confirmed gender accessibility blocker (28 September 2026): the expanded control is exposed
as a button at `(16,401,370,287)` while the picker occupies `(16,471,370,217)`. The button's
centre falls inside the wheel. Before/after diagnostics showed Male selected before the
closing tap and Female in the wheel afterwards. The selector ID is applied to the enclosing
SwiftUI group as well as the row. Required iOS fix: place the interaction ID on the header
button only, give the wheel a distinct ID, and keep any group ID separate. Do not change
gender options, bindings or validation. Automation now rejects overlapping tap bounds and
requires the picker to leave the hierarchy before treating it as closed; `visible=false`
is insufficient on this build. The guard is tested offline against captured bounds. The
full E2E remains blocked pending the app accessibility change and rebuild.

Passenger contract update (28 September 2026): the redesigned hub/editor now use
`PassengerDetails...` IDs, matching iOS source at `9d8d3b2d`. The earlier
`PassengerDetailsOptionOne...` IDs are retired, not passenger-count variants.
Hub, editor, save, DOB, contact, Terms and indexed traveller fields are updated together.
Legacy `NativeFlight...` form selectors remain separate; older OptionOne builds must be
updated before running this suite. No label/coordinate fallback or additional sleep is added.
Editor detection uses its owned scroll viewport as well as its header: an off-screen header
must not switch to legacy selectors. Selection rows target the button with the exact owned
ID, wait for picker closure, and validate complete options (`Male` must not match `Female`).
Country-code evidence includes only labels within the owned row; an already-correct code
is retained without opening the country list. This does not qualify changing country codes.

Local qualification remains **failed**: the renamed hub/editor is now recognised, names and
DOB are entered, but the requested gender is not retained after the close interaction.
The final scoped-row run passed 137 offline checks and failed its one-adult E2E at that
post-selection assertion. The simulator showed Female after Male was requested. Investigate
the app-owned gender button's tap bounds and SwiftUI picker state before further checkout
qualification; this is not evidence of a backend failure. Payment, confirmation and Trips
were not reached. No app source was changed, and the permanent mixed-passenger test is restored.

Carrier hardening (28 September 2026): a local device capture showed SwiftUI propagating
`NativeFlightResult0CarrierVS` onto sibling text/image elements instead of exposing a parent
container. The carrier reader now groups the exact card/carrier identifier in document order
and supports both shapes. It still requires the English departure-airport / carrier-name /
arrival-airport structure, ignores images, strips the additional-carrier count and rejects
missing/ambiguous evidence. It does not infer the operating carrier from marketing text or
another card. A dedicated carrier-name ID remains the preferred long-term app contract.

Failures reported through `SearchBookTestBase` now attempt a pre-teardown `ui-state.xml` under
`TestResults/native-ui-failures/<unique-id>` in the test output directory, locally and remotely.
The bounded snapshot retains native types, Boolean state and allow-listed IDs only; labels,
values, arbitrary names, screenshots and raw page source are not persisted. This reveals
missing/repeated IDs without capturing account or payment content. Capture errors leave the
original failure intact. This structural snapshot does not prove backend health or visible
text correctness. Keep artifacts within the team's test-evidence retention policy.

Validation on 28 September 2026: 130 offline/retry checks passed, and the unmodified
TC24 Virgin Flight Summary scenario passed locally (66 seconds). A temporary one-adult
TC01 checkout retry passed carrier/summary selection, but stopped after opening Passenger
Details: automation expects `PassengerDetailsOptionOneHub`, while the installed app exposes
`PassengerDetailsHub`. Payment, confirmation and Trips were not reached. Reconcile the
redesigned passenger-page contract as a whole before claiming E2E compatibility. The generic
TRX analyser classified this timeout as service availability; the observed page and expected
ID demonstrate a selector-contract mismatch instead. The original mixed-passenger source was
restored, and this run must not be counted as mixed-passenger coverage.

Airport input compatibility: selection first uses the expected app-owned accessibility ID.
When the installed iOS `.searchable` field omits that ID, the bounded interaction helper
accepts exactly one displayed, enabled native search field. Ambiguous matches are rejected.
This temporary exception does not apply to Figma crops; retire it once both input IDs are
reliably exposed. The targeted local run on 25 September passed airport entry and reached
fare selection, then failed waiting for Flight Summary; it was not a successful E2E booking.

Summary compatibility: the itinerary container is optional when legacy origin/destination
context buttons are available. Missing itinerary text remains empty observed fare evidence;
fare assertions must still fail rather than substitute the expected fare. Guest carrier
assertions use the selected flight's scoped operating-carrier code, not aggregate card text.

For shared iOS/Android Search panel scenarios and the proposed Android ID contract, see
[Shared Search panel tests](../docs/MOBILE_ANDROID_SEARCH_PANEL.md). Tests select a page adapter from
the configured session, not duplicated platform scenarios. Six scenarios have local Android passing
evidence in the active pack; journey switching additionally passed targeted Android qualification
after the date-state fix, but remains explicitly skipped in Pending until iOS is revalidated.
The active Android panel pack contains six scenarios. The new readiness scenarios do not submit
a search or join regression. iOS adapter revalidation and BrowserStack Android qualification remain pending.

> Visual comparison uses a bounded Search Panel regression pack. See
> [Mobile visual validation](../docs/MOBILE_VISUAL_VALIDATION.md) for its baseline approval,
> component-capture and diagnostic-artifact guardrails.

The visual layer uses centrally pinned `SkiaSharp` for cross-platform PNG decoding, Retina-aware
component cropping, transparency normalisation, deterministic pixel comparison and expected/actual/
difference artifacts. It does not control Appium or approve baselines. The guide documents its
trust boundary, disposal rules and dependency-audit command.

Implementation benefits and execution evidence are tracked in
[`docs/MOBILE_AUTOMATION_IMPLEMENTATION_BENEFITS.md`](../docs/MOBILE_AUTOMATION_IMPLEMENTATION_BENEFITS.md).

The current test inventory, readiness and coverage boundaries are maintained in
[`TEST-CATALOG.md`](TEST-CATALOG.md). Update it with the machine-readable contract whenever a
scenario is added, renamed, removed, skipped or changes scope.

The iOS source-component map, overlap review and lean E2E decisions are maintained in
[`E2E-COVERAGE-MATRIX.md`](E2E-COVERAGE-MATRIX.md).

All business-facing Smoke E2E and Functional Validation scenarios are additionally marked
`Suite=Regression`. This additive trait builds the full regression pack without replacing their
more specific Category or TestType classifications.

Native iOS and Android end-to-end tests live in this project. Tests use xUnit directly,
Appium for native interaction, and BrowserStack for remote devices. Cucumber feature files
are not required.

## Bounded self-healing

The harness records and reports bounded recovery from known transient mobile mechanics. Current
instrumented recoveries include a SwiftUI filter-sheet rerender consuming the first accessibility
action and the app returning a recognised transient loyalty-login state. Recovery events are scoped
to the Appium driver, so parallel BrowserStack sessions cannot share telemetry.

Each event records its type, operation, bounded attempt, duration and whether the recovery action
succeeded. It is written to xUnit output and annotated on the BrowserStack session. Failure evidence
also reports how many recoveries occurred before the failure. `SelfHealing:WarningThreshold` controls
when a successful session reports repeated instability; it does not convert an otherwise passing
business assertion into a failure.

Self-healing never guesses a replacement selector, changes an expected result, retries permanent
credential/payment/business-rule failures, or swaps journey data after execution begins. New
recoveries must use `MobileHealingPolicy`, remain inside the owning page object, have an explicit
attempt bound and retain the original assertion. See
[`docs/MOBILE_AUTOMATION_HARNESS_AND_AGENT_DELIVERY.md`](../docs/MOBILE_AUTOMATION_HARNESS_AND_AGENT_DELIVERY.md#bounded-self-healing)
for the agent-assisted repair model and safety gates.

## New QA quick start

From `VAA.EAPI.Automation`, ask Codex **"setup the mobile QA tests"** or run:

~~~pwsh
scripts/setup-mobile-app-ui.ps1
~~~

On the first run, the setup workflow checks the .NET 10 SDK, creates
`local.settings.local.json` from the committed safe example, restores/builds the project and
runs offline harness tests. It then lists any placeholders that must be completed and exits
without starting a device session. Edit that file and rerun the same command; when the selected
`RunMode` is complete, it launches one connectivity-only Appium session. It never runs booking
or payment journeys.

Put the BrowserStack username, access key and uploaded application's immutable `bs://` ID in the
generated local settings file. The script reports missing values without displaying their
contents.
The committed `appsettings.json` contains safe shared defaults. The gitignored
`local.settings.local.json`, created from `local.settings.local.example.json`, uses the same
configuration shape and overrides values specific to the QA's app build or workstation. It is
gitignored because it contains BrowserStack credentials. Never remove that ignore rule or commit
the populated file.

For a BrowserStack run, the QA must make these changes in the generated
`VAA.EAPI.Automation.MobileApp.UI/local.settings.local.json`:

1. Set `RunMode` to `Remote`.
2. Replace `BrowserStack.Username` and `BrowserStack.AccessKey` with the QA's credentials.
3. Replace `BrowserStack.App` with the immutable `bs://` ID returned by the BrowserStack app
   upload. Do not use the upload filename or dashboard URL.
4. Optionally update `BrowserStack.Build` and `BrowserStack.Name` to make the run easy to find.

For a local run, set `RunMode` to `Local` and select `Platform=iOS` or `Platform=Android`.
Setup validates only that platform's block:

- iOS: `ServerUrl`, `DeviceName`, `PlatformVersion`, `UDID`, `BundleId`.
- Android: `ServerUrl`, `DeviceName`, `UDID`, `AppPackage`, `AppActivity`.

Inactive-platform placeholders are ignored; missing/unknown platforms are rejected explicitly.
The Android example targets an installed app; `App` is optional and `PlatformVersion` is discovered
by UiAutomator2. This validation is offline-tested, but the complete setup connectivity workflow
remains iOS-focused and has not been qualified on Android. Use the documented
[Android panel execution path](../docs/MOBILE_ANDROID_SEARCH_PANEL.md) for the initial Android work.

The BrowserStack block may remain in the file because it is ignored in local mode. Conversely,
the Appium block may remain while `RunMode` is `Remote`.

`Platform` selects the local Appium block (`Appium.iOS` or `Appium.Android`), whereas `BrowserStack.Platform`
selects the remote BrowserStack platform. Both are present in the example because the same file
supports switching between local and remote execution.

To prepare and verify the project without consuming a BrowserStack device session:

~~~pwsh
scripts/setup-mobile-app-ui.ps1 -SkipConnectivityTest
~~~

Do not use an unfiltered `dotnet test` as a setup check. This project contains real E2E
journeys that proceed through booking and payment.

### Supported execution combinations

| QA machine | Target | Support |
| --- | --- | --- |
| Windows | BrowserStack iOS device | Supported and recommended for Windows-based QAs. No local Xcode installation is required. |
| macOS | BrowserStack iOS device | Supported. |
| Linux | BrowserStack iOS device | Supported. |
| macOS | Local iOS simulator or connected device | Supported when Xcode, Appium and the XCUITest driver are installed and configured. |
| Windows or Linux | Local iOS simulator or connected device | Not supported because Xcode and the iOS simulator require macOS. |
| Windows, macOS or Linux | BrowserStack Android device | The shared driver supports Android, but the current connectivity and Search and Book tests are iOS-focused; Android execution is not yet a proven QA path. |

For the current supported newcomer workflow, a Windows QA runs the iOS app remotely on a
BrowserStack device. They need PowerShell 7, the .NET 10 SDK, BrowserStack credentials and
the uploaded iOS application's `bs://` ID. They do not need Appium, Xcode or an iOS
simulator installed locally.

## Configuration

Committed `appsettings.json` contains non-secret defaults. Copy
`local.settings.local.example.json` to `local.settings.local.json`, then use that single
gitignored file to choose local or BrowserStack execution and supply machine-specific settings.

Required for a BrowserStack run:

- `BrowserStack.Username`
- `BrowserStack.AccessKey`
- `BrowserStack.App` — immutable `bs://` application URL

Common optional overrides:

- `BrowserStack.DeviceName`
- `BrowserStack.PlatformVersion`
- `BrowserStack.DeviceFallbacks` (ordered alternatives used only when BrowserStack session
  creation fails transiently)
- `BrowserStack.Build`
- `BrowserStack.Name`
- `BrowserStack.Profile` (`Diagnostic` by default; use `Fast` for local feedback)
- `BrowserStack.AppiumVersion` (pinned deliberately; validate changes with connectivity and E2E)
- `BrowserStack.CommandTimeoutSeconds` (client command timeout; defaults to 240)
- `BrowserStack.IdleTimeoutSeconds` (BrowserStack/Appium inactivity timeout; maximum 300)
- `BrowserStack.SessionCreationAttempts` (defaults to 3; session creation only)
- `BrowserStack.SessionCreationRetryDelaySeconds` (defaults to 5)
- `BrowserStack.DisableAnimations` (defaults to true)
- `BrowserStack.ForceAppLaunch` (defaults to true for iOS so launch-time feature flags are
  reapplied even when BrowserStack preserves the installed app)
- `BrowserStack.NetworkLogs` and `BrowserStack.CaptureNetworkContent` (network logs default to
  true and content capture defaults to false; keep network logs enabled and opt in to content
  capture for a targeted diagnostic run, where safe summaries are emitted only on failure)
- `IOSLaunch.BuildProfile` (`sit_standard` by default)
- `IOSLaunch.AutomationFeatureFlags` (comma-separated,
  deduplicated app-owned flag names passed before launch)

The tracked BrowserStack chain matches WDIO: iPhone 17/iOS 26, then iPhone 15/iOS 26,
then iPhone 14/iOS 26. Fallback is limited to transient session-creation failures such as
device allocation. A functional failure is never retried on another device.

Do not commit credentials, payment data, or other secrets.

### Authenticated loyalty Search and Book

Account tests reuse the EAPI `LoyaltyAccounts` Azure Storage table and Key Vault connection
already used by backend automation. Each test supplies its own environment, tier and points
criteria in code; those values are not read from settings. Add only the shared connection and
password values below to the gitignored `local.settings.local.json` before running the tests:

- `Loyalty.Password` — the shared test-account password. This secret belongs only in the local
  settings file or the equivalent secure pipeline configuration, never in committed settings.
- `Core.KeyVaultUrl` — the EAPI test Key Vault used by the existing Core storage helper.
  `Core.KeyVaultTenantId`, `Core.KeyVaultClientId` and `Core.KeyVaultClientSecret` are optional;
  leave them empty to use the developer or workload identity already available to Azure's
  default credential chain.

Run the authenticated journeys with:

~~~pwsh
dotnet test VAA.EAPI.Automation.MobileApp.UI/VAA.EAPI.Automation.MobileApp.UI.csproj -c Release --filter "Category=MobileApp-Account-SmokeE2E" --results-directory TestResults/mobile-account --logger "trx;LogFileName=mobile-account.trx"
~~~

The previously duplicated Red, Silver and Gold booking scenarios are consolidated into one
representative member journey. TC01 is now enabled as the high-balance Economy redemption E2E.
It leases a SIT Red account with at least 100,000 table points, explicitly enables reward search
for one adult LHR-JFK, and selects a Virgin-operated Economy fare. Before checkout it checks
the positive fare cost against the live member balance, its retention on Summary and enabled
Continue. It reuses the sole-member prepopulated passenger flow, submits HPP payment for the
cash component, checks confirmation and finds the reference in Trips. This consumes test points
and creates a test booking: run only against an approved test environment. Payment failure remains
terminal, with no automatic retry. Table balance is a selection hint, not proof of affordability.
Backend ledger deduction/reconciliation is not asserted by this native journey. Tier retrieval
remains shared; the booking behaviour is not duplicated across tiers.

Local TC01 execution on 28 September passed login and Economy affordability (30,000 points
against 150,000 live points), Summary and passenger steps. After one HPP submission the app
displayed `Try secure payment again`; the test failed without retrying. Confirmation and Trips
remain unverified for this redemption run. The implementation is enabled, not a claimed E2E pass.

The loyalty method names follow the harness convention
`<Area>_TC##_<JourneyOrScope>_<ExpectedBehaviour>`; environment selection remains in setup and
traits rather than being embedded in the method name:

- `LoyaltySearchBook_TC01_HighBalanceEconomyRedemption_CompletesThroughHppConfirmationAndTrips`
- `LoyaltySearchBook_TC02_LowBalanceReturnUpperClass_BlocksContinuationForInsufficientPoints`
- `LoyaltySearchBook_TC03_CurrencyBooking_SelectsDisplayedPayWithPointsOption`
- `LoyaltyAccount_TC01_StoredMemberFirstName_MatchesPersonalDetailsAndExposesAccountActions`

`LoyaltySearchBook_TC02_LowBalanceReturnUpperClass_BlocksContinuationForInsufficientPoints`
covers the negative reward
boundary without reaching passenger details or HPP. It leases a SIT Red account with fewer than
50,000 points, explicitly enables reward search, selects outbound and inbound Upper Class fares,
and reads the outbound price plus inbound adjustment rather than assuming a fixed reward price.
The test proves the resulting displayed cost exceeds the leased balance, checks the app-owned
`NativeFlightSummaryInsufficientPoints` warning and verifies the Flight Summary Continue button
is disabled. It is marked `Readiness=Ready`; the local SIT run passed on 2 September 2026.

`LoyaltySearchBook_TC03_CurrencyBooking_SelectsDisplayedPayWithPointsOption` covers the
logged-in Pay with points journey. It leases a SIT Red account with at least 3,000 points,
explicitly switches off Search reward seats, performs a normal one-way currency search and
reuses the authenticated passenger flow. At payment handoff it opens the app-owned points
selector and chooses the first option in the deterministic displayed order. It deliberately stops
before applying the contribution; applied totals, HPP, confirmation and Trips remain deferred
until the app's post-apply behaviour is ready for reliable automation. The scenario is a ready
functional-validation test rather than an end-to-end booking test.

The Pay with points page object uses only the iOS app-owned
`NativeFlightPayWithPoints...` identifiers. Option controls are addressed by their zero-based
display index; points and monetary values remain observable labels or values and are never used
as selectors.

`LoyaltyAccount_TC01_StoredMemberFirstName_MatchesPersonalDetailsAndExposesAccountActions` ports the
WDIO Account Settings Personal Details scenario. It leases a Red SIT account from the shared
`LoyaltyAccounts` Azure Storage table, logs in, opens Settings and Personal Details, and checks
that the stored first name appears in the app-owned profile title. The non-editable `firstNames` and
`lastNames` controls currently expose their field labels rather than their stored values to
Appium, so they cannot provide a truthful assertion target without an app accessibility change. It also
checks that Edit details and Delete account are available without opening either external
account-management journey. The source WDIO scenario claims first-and-last-name coverage but
conditionally skips the surname assertion when the title contains only one name, which is the
current iOS behaviour. The .NET contract names the proven first-name scope explicitly and records
surname validation as blocked until the app exposes the non-editable field value. The account
lease also prevents another parallel test in the same process from using the same member.

Authenticated checkout deliberately uses a separate passenger-data mode from guest checkout.
The app selects the logged-in member as the first passenger by default and prepopulates that
passenger's traveller and contact details, as it does for a redemption booking. When the member
is travelling and is the only passenger, automation must not edit, reselect or scroll through
those fields: it only accepts the booking terms and continues. This rule applies to redemption,
currency and Pay with points journeys.

If a future scenario uses the passenger dropdown to select that the logged-in member is not
travelling, the first passenger is no longer covered by this shortcut. That scenario must use a
separate page-object path which selects the alternative traveller and completes or verifies the
new passenger fields. Do not reuse the sole-member path or infer the state from passenger count
alone; the dropdown selection determines whether the first passenger is prepopulated.

Scrollable screens own their container contract. Passenger Details uses
`NativeFlightPassengerDetailsScrollView` and bounded container-scoped scrolling to reach booking
terms without targeting or focusing traveller fields. Calendar scrolling remains scoped to its
separate calendar container; test authors must not substitute screen-wide or named-element
scrolling across these pages.

The redesigned passenger hub uses `PassengerDetailsHub` as its scroll container. Terms and
Continue now reuse `ScrollWithinContainerToKey`, with at most eight directional scrolls and
a fresh container lookup per attempt, rather than the generic named-element/screen-wide helper.
This does not heal an unresponsive app accessibility tree or retry checkout/payment. On 28
September, the three-passenger TC03 passed the full E2E through payment, confirmation and Trips
(4m50s; `TestResults/passenger-hub-return-20260928/return.trx`), while five-passenger
TC01 still stalled at the hub: WDA waited for an accessibility snapshot and an app stack sample
showed SwiftUI layout activity. The underlying five-passenger cause remains unproven and needs
app-side investigation; do not mark TC01 fixed based on offline checks or smaller parties.

The planned Passenger Details redesign must preserve this fast path and meet the native automation
contract at delivery time. It must expose stable app-owned identifiers for the screen, passenger
container, traveller selector, each editable field, validation messages, terms control and Continue
action. Controls must expose their current value and selected/enabled state to Appium, and Continue
must become enabled from observable form state without animation-dependent delays. Repeated
passengers must use deterministic displayed indexes rather than generated identifiers. The sole
logged-in traveller must remain verifiable without opening the passenger container; tests should
only traverse fields when the member selects that they are not travelling or the app reports
incomplete required data.

After passenger submission the flow accepts either direct payment handoff or seat selection;
when the app presents seats, it continues without selecting one before opening secure payment.

Each scenario leases one eligible account for its lifetime, preventing parallel tests in the
same process from mutating the same member. Logs expose only the tier and final four Flying Club
digits. The login and booking flows use app-owned accessibility IDs, poll explicit success/error states,
does not retry invalid credentials, and retries only the app's known transient error. A login
that completes on another tab is brought back to Account before member-only greeting and points
controls are checked. Both the Account guest `guestLoginButton` and landing `Sign in` entry
states are supported because the app can expose either after startup. Cleanup returns to Account before logout, including after a completed
journey leaves the app in Trips. Fixed sleeps from the legacy WDIO flow are deliberately not
carried over.

Test authors must request data explicitly in the scenario; there is deliberately no global
fallback for environment, tier or points:

~~~csharp
using LoyaltyAccountLease account = provider.Lease(new LoyaltyAccountCriteria(
    Environment: "SIT",
    Tier: "Gold",
    MinimumPoints: 50_000,
    MaximumPoints: 100_000)); // optional, exclusive
~~~

Use `LoyaltyAccountCriteria.WithExactPoints("SIT", "Gold", 50_000)` when an exact integer
balance is required. The typed provider translates the request to Core's existing
`RetrieveLoyaltyAccountNumberDetailsFromStorageTable` helper, then validates the returned tier
and points before login. Azure connection and filtering logic therefore remain centralised in
Core rather than being reimplemented in the mobile project.

Compared with the inspected WDIO login flow, this implementation adds typed and validated
account criteria, process-local account leasing, member-only state detection, masked identifiers,
local-specific failure output and deterministic logout cleanup. It retains WDIO's useful known
transient/invalid-credential outcomes but replaces its fixed login and post-login pauses with
explicit polling. The rationale and local Red/Silver/Gold execution evidence are recorded in
[`docs/MOBILE_AUTOMATION_IMPLEMENTATION_BENEFITS.md`](../docs/MOBILE_AUTOMATION_IMPLEMENTATION_BENEFITS.md#loyalty-login-implementation-evidence).

For a targeted backend diagnostic run, keep `BrowserStack.NetworkLogs` set to true and opt in by
setting `BrowserStack.CaptureNetworkContent` to true in the gitignored local settings file before
the BrowserStack session is created. Content capture is false in committed defaults and examples
to avoid retaining response bodies during ordinary runs. The
normal test run retrieves and emits sanitised operation/status/code/correlation metadata only
when that original session fails. This is necessary because BrowserStack cannot enable content
capture retrospectively, and retrying a failed payment can lose the original backend evidence.
The optional `scripts/run-mobile-search-book-diagnostics.ps1` wrapper now also performs one run
only; it never retries payment or reruns the failed scenario. Raw request and response bodies are
never written to test output.

`Diagnostic` keeps BrowserStack video, visual debugging and network logs enabled. `Fast`
disables those three high-overhead capture features while retaining Appium logs and stable
element waits. Optional `BrowserStack.Debug`, `BrowserStack.Video` and
`BrowserStack.NetworkLogs` values in the local settings file override the selected profile.

Use profiles according to the scope of the test:

- Use `Fast` for focused, deterministic UI checks such as Search entry, calendar boundaries
  and passenger-selection rules. If backend evidence may be required, explicitly retain network
  logs even when the remaining Fast-profile video/debug capture is disabled.
- Use `Diagnostic` for full booking E2E journeys through passenger details, HPP payment,
  confirmation and Trips. Video and visual evidence are valuable when diagnosing a failure
  across native and hosted-payment transitions.
- Use a new diagnostic run only when the original test was started without network capture; it
  cannot recover evidence from that earlier session.

The profile must be chosen before the BrowserStack session is created; video cannot be
enabled retrospectively for an active or completed session.

When explicitly enabled together, the two network settings make response bodies available inside BrowserStack so HTTP
200 responses can still be checked for GraphQL `errors`. The harness reads those logs only from
a failed session and emits allow-listed metadata: HTTP status, safe host/path, GraphQL operation,
backend error code and correlation ID. It never prints raw bodies, query values, passenger data,
credentials or payment data.

On BrowserStack failure, `SearchBookTestBase` writes a safe correlation block to xUnit/TRX output and
annotates the BrowserStack session. It includes the session ID, project, build, app, profile,
network-content setting and a basket ID only when an API-assisted flow has explicitly supplied
one. Native Appium cannot reliably read the app's internal basket state, so an absent value is
reported honestly rather than guessed.

Local Appium/XCUITest does not expose the native app's HTTP traffic. Local failures therefore
report the explicit app-owned error state and session/device correlation, while the same scenario
must run on BrowserStack for the sanitised HTTP/GraphQL failure summary. A local interception
proxy would be a separate opt-in diagnostic setup and is not required by this harness.

### Local iOS app logs on failure

For full-screen failure context, separately enable
`Diagnostics.FullScreenScreenshotOnFailure` in gitignored `local.settings.local.json`
(committed default/example: false). The failure handler takes one Appium screenshot before
other evidence collection and teardown, without navigating, retrying or changing context.
It saves `TestResults/mobile-failure-screenshots/<id>/screen.png` beneath the test output
directory and prints its path in xUnit/TRX output (not an embedded attachment).
This uses the driver screenshot command for local or BrowserStack sessions on iOS/Android;
live execution still requires qualification. Passing tests save nothing,
and capture failure does not replace the test failure.

Full-screen images are **unredacted** and may expose passenger, loyalty, login or payment
information. Enable only for approved synthetic-data investigations, disable afterwards,
review/redact before sharing, and delete local images after investigation. Do not commit
or automatically publish them as CI artifacts. This option is independent of the restricted
error-control crops below; those crops never implicitly enable full-screen capture.

Failure screenshots are separately opt-in via `Diagnostics.ErrorControlScreenshotsOnFailure`
in gitignored `local.settings.local.json` (default/example: false). The current implementation
captures cropped native iOS text/button controls with `seatSelection.error` or
`NativeFlightPaymentRetryButton` IDs before log collection and teardown. These are error crops,
not full-page screenshots or automatic PII redaction. Review the crops before sharing; app error
text itself must remain non-sensitive. Passenger/payment input screens, unknown IDs, container
elements, web contexts and Android are excluded; no full-screen fallback is used.

Images are saved under `TestResults/mobile-error-screenshots/<id>/error.png` in the test output
directory. Each path is printed in xUnit/TRX output; images are separate files, not embedded TRX
attachments. Successful tests save nothing. Screenshot errors never replace the original failure.
Keep these local artifacts private and remove them after investigation. This also works through
the same handler for an iOS BrowserStack session when enabled, but remote capture is not yet
qualified. Historical failures cannot be reconstructed; capture starts with the next failed run.

`Diagnostics.IosAppLogsOnFailure` defaults to true. Before failure teardown, the harness reads
the local Appium XCUITest `syslog` buffer and filters the exact executable named by
`Diagnostics.IosAppProcessName` (default `vaa-mobile-ios`, not its bundle ID). The committed
defaults apply to existing local settings; override those keys in `local.settings.local.json`
only when disabling capture or changing the executable name. Android and BrowserStack skip this
collector; BrowserStack's existing network diagnostics are unchanged.

The Appium driver starts log capture with the session by default (`skipLogCapture=false`). Keep
`showIOSLog` disabled and do not turn on raw syslog mirroring to a file. The harness retrieves
logs only after a test reports failure, before logout/teardown, and writes a unique
`TestResults/ios-app-logs/<id>/app-log-summary.txt` under the test output directory. Its path and
sanitised summaries also appear in xUnit/TRX output. Successful tests create no log artifact.

This is **not raw HTTP capture or a copy of Xcode's debugger console**. Only recognised status
codes, UUID correlation/session/operation IDs, W3C traceparent values, a closed set of error
codes and generic error signals are retained. Arbitrary messages, unknown code values, URLs,
request/response bodies and stack traces are omitted to protect credentials and passenger/payment
data. Correlation is a diagnostic lead, not proof that a logged error caused the test failure.
No Application Insights lookup is implemented yet.

### Tracing a failure without changing the app

Failure output includes a UTC investigation window from diagnostic-context creation to the
failure, captured before evidence collection. Use that window with the Appium session and
sanitised log artifact. BrowserStack summaries retain request and response correlation headers
separately, including UUID-valued `api-correlation-id`, `api-operation-id` and `api-session-id`.
Different identifiers are not silently treated as the same request.

For a backend investigation, supply the SIT Application Insights resource name or portal link
and an approved read-access path. Start with an observed correlation identifier; if none exists,
searching the UTC window yields candidates, not proof of which request failed. Confirm the
operation and its linked dependency failure before reporting a root cause. Do not export raw
payloads, credentials or passenger data. No query client, proxy, certificate or iOS diagnostic
instrumentation is installed automatically. A previous local request cannot be recovered from
Appium if neither the app logs nor backend telemetry retained matching evidence.

Retrieval is limited to one loopback HTTP request, eight seconds and four MiB in memory, with
no redirects or proxy. At most the final 2,000 driver entries are inspected; only entries since
test initialisation and within the last five minutes qualify, with at most 40 summaries saved.
Oversized, unavailable or malformed buffers produce an unavailable message without replacing
the original failure. An empty summary does not establish that the app/backend had no errors.
Appium's log buffer may be truncated or consumed by another reader. Swift `print` output seen
only under Xcode's debugger may not appear; app-owned structured logging would then be needed.
This collector never repeats a journey/payment, changes app functionality or writes raw logs.

Qualification on 28 September 2026: 196 offline/retry checks passed
(`TestResults/ios-log-evidence-final-20260928/offline.trx`). A temporary non-booking local
Appium session successfully read `syslog` (4,663 entries, 2,695 matching the app executable);
the probe emitted counts only and deleted its session. This proves driver log availability,
not that the HPP error or a correlation ID is logged. No payment was rerun for diagnostics.

## BrowserStack reliability model

Each test scenario owns a fresh Appium session. The `Mobile` collection remains sequential
by default, so isolation does not unexpectedly consume the shared BrowserStack device pool.
This prevents a failed connection or app state from contaminating the next scenario.

Search and Book tests rename their BrowserStack session at test start using
`TestClass | TestMethod`. Before teardown, every scenario explicitly marks the session
`passed` or `failed`; failures include the exception type and deepest available root cause.
BrowserStack may still record `CLIENT_STOPPED_SESSION` as the termination reason because normal
cleanup calls `driver.Quit()`. That value describes how the WebDriver session ended and must not
be used as the test outcome; the explicit Automate status and reason are authoritative.

Refinement tests also restore Recommended sorting and clear selected filters through the app UI
before their assertion. Do not clear simulator app data between tests: it can discard the
launch-time feature flags and leave the test app unable to reactivate.

The harness retries only recognised transient failures while creating a session. It never
retries an in-progress booking, UI assertion, GraphQL/PRI error, or payment failure; those
remain visible product or automation failures. BrowserStack's idle timeout is explicitly
set to its supported 300-second maximum, while element and journey waits continue to poll
for real UI state rather than sleeping.

Normal teardown calls `driver.Quit()`. If that call fails, the harness logs the failure and
uses an authenticated WebDriver DELETE as a bounded fallback so an orphaned session does
not retain a shared device. Credentials remain in the gitignored local settings file and are
never written to reports.

`AppiumVersion` is intentionally pinned. The BrowserStack iOS pin is `3.3.0`, currently the
newest Appium 3 version BrowserStack documents for iOS. Any future upgrade remains a deliberate
compatibility change and must be validated with connectivity plus one complete booking journey.

These settings follow BrowserStack's documented
[Appium timeout](https://www.browserstack.com/docs/app-automate/appium/references/timeouts),
[idle-timeout](https://www.browserstack.com/docs/app-automate/appium/troubleshooting/browserstack-idle-timeout),
and [Appium-version](https://www.browserstack.com/docs/app-automate/appium/set-up-tests/set-appium-version)
guidance.

For focused checks, set `BrowserStack.Profile` to `Fast` in `local.settings.local.json`.

For a full booking E2E run:

Set `BrowserStack.Profile` to `Diagnostic`. The failure-only diagnostic runner manages its
temporary capture overrides automatically; users do not need to configure environment variables.

## Run the iOS Search and Book suite

The suite has two intentionally distinct groups:

- `MobileApp-SearchBook-FunctionalValidation` — TC05 checks search-entry rules. Refinement coverage is split
  into focused sort, filter, switching, persistence and Clear/reset classes. Airline-filter
  checks use carrier evidence on each result card and open Flight details only when the card
  does not show the selected carrier. TC07 records six origin/destination pairs within one
  Appium session, then verifies the independent origin and destination Recent Searches lists
  retain exactly the latest five airports in newest-first order. These are functional tests,
  not launch smoke tests.
- `MobileApp-SearchBook-SmokeE2E` — TC01 and TC03 are complete guest booking journeys through
  results, payment, confirmation and Trips. TC02 and TC04 stop at Flight Summary after proving
  their distinct partner and mixed-carrier selections, avoiding duplicate checkout coverage.
  Each scenario owns a separate test class and Appium session, allowing BrowserStack to run
  all four journeys concurrently. `RunMode=Local` uses the harness session gate to run them
  one at a time against the single configured simulator.

Before either suite, run the connectivity-only check. It creates the Appium session and
verifies that the app reaches a recognised onboarding, primary-navigation, or native Search
& Book entry point. Local iOS sessions use `ForceAppLaunch=true` to terminate and relaunch the
installed app with the configured feature arguments while preserving its data. Each refinement
test then clears filters and restores Recommended sorting through the UI. Do not use `clearApp`
or `FullReset` for routine local execution; those operations can remove app state required to
relaunch an installed test build. Recent Searches intentionally creates and verifies its state
within one test session:

~~~pwsh
dotnet test VAA.EAPI.Automation.MobileApp.UI/VAA.EAPI.Automation.MobileApp.UI.csproj -c Release --filter "Category=MobileApp-Connectivity" --results-directory TestResults/mobile-connectivity --logger "trx;LogFileName=mobile-connectivity.trx"
~~~

Run the focused functional validation checks:

~~~pwsh
dotnet test VAA.EAPI.Automation.MobileApp.UI/VAA.EAPI.Automation.MobileApp.UI.csproj -c Release --filter "Category=MobileApp-SearchBook-FunctionalValidation" --results-directory TestResults\mobile-search-book-functional-validation --logger "trx;LogFileName=mobile-search-book-functional-validation.trx"
~~~

Run regression by business area:

~~~pwsh
# Search and Book, including authenticated loyalty booking journeys
dotnet test VAA.EAPI.Automation.MobileApp.UI/VAA.EAPI.Automation.MobileApp.UI.csproj -c Release --filter "Suite=Regression&RegressionArea=SearchAndBook"

# My Account only
dotnet test VAA.EAPI.Automation.MobileApp.UI/VAA.EAPI.Automation.MobileApp.UI.csproj -c Release --filter "Suite=Regression&RegressionArea=MyAccount"

# Online Check-In (separate project)
dotnet test VAA.EAPI.Automation.MobileApp.OCI.UI/VAA.EAPI.Automation.MobileApp.OCI.UI.csproj -c Release --filter "Suite=Regression&RegressionArea=OnlineCheckIn"
~~~

The focused functional classes run with at most four concurrent BrowserStack sessions; every
scenario owns its own Appium session, while all steps within TC17 deliberately share that
scenario's session so its recent-search history is preserved. Four provides useful suite speed
without allowing the runner's CPU count to overload BrowserStack capacity or the shared search
backend. Reassess this explicit `xunit.runner.json` limit using observed stability and available
BrowserStack concurrency before increasing it. `RunMode=Local` automatically serializes mobile
sessions so a single simulator and WebDriverAgent are not driven by competing tests.

Run the end-to-end smoke journeys:

~~~pwsh
dotnet test VAA.EAPI.Automation.MobileApp.UI/VAA.EAPI.Automation.MobileApp.UI.csproj -c Release --filter "Category=MobileApp-SearchBook-SmokeE2E" --results-directory TestResults\mobile-search-book-smoke-e2e --logger "trx;LogFileName=mobile-search-book-smoke-e2e.trx"
~~~

Search and Book tests are direct xUnit tests, with locators and interactions kept in page
objects.

### Authoring parallel-safe functional scenarios

Completed-booking scenarios must call `ReportSuccessfulBooking(booking, reason, testName)`
after their final assertions. This shared base method writes the standard
`[confirmed-booking] PNR=...; scenario=...` record to xUnit/TRX before the BrowserStack
status update. Slack reads this record only for passed tests; do not duplicate booking-reference
logging in individual tests. The record includes no passenger or payment details.

Treat an independently runnable user scenario as the unit of test isolation and remote
parallelism:

- create one concrete test class containing one scenario `[Fact]`;
- give every independently runnable scenario a unique sequential `TC##` identifier within its area;
- retain the `SearchBook_TC##_Outcome` method naming and required category, test-type, journey or
  feature and platform traits. Regression scenarios require `Suite=Regression` and
  `RegressionArea=SearchAndBook`; deliberately short pre-merge checks use `Suite=Smoke` and
  `Area=SearchAndBook` instead;
- keep the whole user outcome in that test, including payment, confirmation, and Trips where the
  acceptance criterion requires them;
- discover inventory-driven filter options (such as airlines) from the current results, then
  apply and validate each offered option; do not assume a fixed carrier will always be returned;
- move repeated mechanics into page objects, flows, or an abstract test base rather than placing
  several scenarios in the same concrete class;
- do not use ordered/dependent tests and do not add parallel-capable scenarios to the
  non-parallel `Mobile` collection.

xUnit can run the resulting classes concurrently on BrowserStack, with one Appium session per
scenario. For `RunMode=Local`, `MobileFixture` automatically gates session creation so the same
suite runs one class at a time against the single simulator. Authors and agents should not add a
separate local-only collection or globally disable parallel execution.

`Contracts/search-book-automation-contract.json` is the machine-readable coverage and selector
inventory for coding agents. Offline contract tests fail when TC01-TC30 cases no longer map one-to-one
to categorised xUnit tests, when selector ownership drifts, or when platform support is described
ambiguously. When asking Codex to create automation from a PR, issue, or acceptance criteria, use
`$mobile-qa-authoring`; use `$mobile-qa-setup` only for workstation setup and connectivity.

TC24-TC26 validate the redesigned Flight Summary for Virgin-operated, partner-operated and
non-direct multi-carrier (`+1`) results. They currently stop at the summary boundary to keep failures
focused and fast. Treat these assertions as shared stages for later E2E journeys: once the downstream
app flow is delivery-ready, reuse them before continuing through passenger details, payment,
confirmation and Trips rather than duplicating summary checks in the E2E test.

TC27 validates the app-owned flexible-date carousel and TC28 validates the app's inbound-only Edit
Search route. Both are ready but have not been run while the known downstream search issue remains.
TC29 is `Readiness=Ready`, verified locally against the rebuilt iOS app on 28 September 2026.
It reads the selected fare title and included/not-included benefit rows, checks the retained Summary
fare, then opens `NativeFlightSummaryItinerary0FlightDetails` and checks the baggage inclusion and
advertised count/weight. It no longer navigates through Price / Fare conditions. Fare-title IDs and
the baggage row's category accessibility value must be present. Assertions currently support English
labels; dimensions, fare-rule text and exhaustive carrier/connecting-fare combinations are not covered.
On 28 September the old test reached the price sheet but failed on the missing fare-conditions link;
the replacement flow passed locally in 56 seconds. Explicitly excluded baggage may have no Summary
allowance row; a positive allowance for that category still fails. Included benefits require matching
allowances and advertised count/weight. All exact indexed allowance IDs are read from the hierarchy,
including off-screen rows, without assuming contiguous indexes. TC30 documents
controlled search recovery and remains
skipped until the test app offers a deterministic fail-once service hook. Controlled payment failure
coverage remains explicitly deferred in `E2E-COVERAGE-MATRIX.md`.

## iOS automation contract

The current implementation is aligned to
[`vaa-mobile-ios` PR 1040](https://github.com/VAAD1ST/vaa-mobile-ios/pull/1040) on branch
`feat/search-and-book-design-summary`, commit
`3f62f8ced7b9c14946c9818637ccaa09f5e33fc7`. The canonical app-owned selector inventory is
`docs/SearchAndBook/native-search-and-book-automation-contract.md` in that repository.

Automation readiness includes both interaction and visual ownership. Controls used for Figma
comparison must expose a stable accessibility ID on the native element owning the complete
visible component bounds, in addition to IDs on interactive children. A text-field ID attached
only to its inner input cannot identify its surrounding outlined field; individual passenger
counter IDs cannot identify the complete passenger sheet. Authors must record these as app
contract gaps rather than introducing coordinate crops, hierarchy selectors or permissive visual
thresholds. See [`docs/MOBILE_AUTOMATION_FRAMEWORK_DECISION.md`](../docs/MOBILE_AUTOMATION_FRAMEWORK_DECISION.md#visual-automation-readiness-contract)
for the development-PR readiness checklist.

`NativeSearchAccessibilityIds` contains only identifiers implemented at that commit.
Calendar-day selection and passenger-selector confirmation remain isolated interim
label/structure fallbacks because the app contract records that deterministic IDs are not
yet implemented. Exact result/fare selection, traveller fields, seats, payment actions and
confirmation assertions must migrate to the required indexed IDs as the app team supplies
them; generated UUID identifiers must not become permanent automation selectors.

## Harness interaction conventions

- TC05 checks the inclusive `today + 330 days` booking horizon and rejects day 331,
  not a calendar-year boundary. It still requires the actual day control and its enabled
  state; a missing date is not silently accepted as disabled.
- Guest-session setup and login share Account-entry navigation. After a tap, wait for a
  member, guest, landing or login state. Only an absent destination permits up to two
  additional taps, recorded through `MobileHealingPolicy`. Lookup/tap failures and transport
  errors are not retried; logout, credential submission and promo assertions remain outside
  this recovery. A successful recovery tap does not itself prove destination readiness.
  Only a dedicated readiness-expiry exception permits recovery. A command timeout inside
  the readiness poll (including privacy dismissal) propagates unchanged without a re-tap.
  A visible Account load error fails immediately. Until the app supplies an owned error ID,
  this uses the exact English accessible name `Something went a little off course`, matching
  the existing login-outcome copy. Replace this narrow fallback when the ID is delivered.

- TC18 discovers the exact Economy Classic title and uses that fare's indexed Select button
  and `NativeFlightFareSelectorFare{index}AppliedPromotion` badge. The old selector-wide promo
  ID is not part of the current app contract. Required promo text is condition-waited; missing
  or conflicting evidence must fail, never fall back to another fare's badge or the results card.
  The test retains the unpromoted baseline, matching itinerary, GBP price and exact £5 reduction
  assertions. HEMAN eligibility for the next May remains a controlled test-data dependency;
  Economy Light and checkout/payment are outside TC18's scope.

- Results-route checks preserve endpoint order using the same ordered header parser as Flight
  Summary. Carrier-code classification requires the scoped result identifier and never infers VS
  from generic card text. Filter element, reset and page-source checks share the same selected-state
  rule (`selected=true`, `value=1` or `value=true`), so an unchanged native value cannot falsely
  confirm deselection.

- Guest contact-entry timing follows the passenger-page layout: legacy forms complete every
  traveller before scrolling to the shared contact section; redesigned editors complete contact
  details before saving the first traveller. Legacy cards are scrolled into view using their own
  IDs; an off-screen card is not evidence of a redesigned page.
- TC26 retains the card's primary carrier, stop count and `+1` checks, then uses the scoped
  per-leg IDs from iOS `feature/search-book-leg-accessibility`. It opens the explicit Flight details
  action before fare selection and from Summary; ordered airports, full dates, clocks, operators
  and optional descriptors must agree. Adjacent airports and connection airport values must match;
  endpoints and first departure date match the search. Missing/conflicting values fail explicitly.
  Bounded container scrolling replaces positional guesses. This is retention evidence, not
  independent backend truth or UTC chronology. The extension is blocked on the installed app contract;
  earlier TC26 passes cover only its older assertions. Filter tests retain their card-first path.

- Summary assertions use observed text only; a missing fare must fail, never be replaced by
  the expected selection. Ordered route-header parsing currently supports the English LHR/JFK/MAN
  journeys and preserves origin/destination order. City-only headings do not independently prove
  the exact airport; dedicated summary endpoint IDs remain the preferred app contract.
- Result carrier evidence is scoped to `NativeFlightResult{index}Carrier{code}`. The current iOS
  `DetailedFlightRowInfoView` puts that ID on the whole flight-information block, so a temporary
  structural reader extracts the airline label between the English departure/arrival airport
  labels. It fails on missing/ambiguous evidence instead of using the aggregate card text or a
  bare carrier-code substring. Replace this reader when a dedicated carrier-name ID is delivered;
  the required app change is tracked here rather than silently widening selector fallbacks.
- Default refinement reset clears only when an app-owned filter option is selected; otherwise
  it applies the unchanged sheet. It then restores Recommended sorting.

- Locate native controls by stable accessibility ID.
- Calendar validation reads the app-owned departure and return summary IDs after every
  selection, then waits for the calendar sheet to dismiss after an enabled Next action.
  Boundary inspection deliberately cancels and reopens an uncommitted calendar before the
  valid range is selected; the cancel button is not a happy-path substitute for a valid range.
- Calendar navigation uses the exact ISO day accessibility ID and scrolls only
  `NativeFlightCalendarScrollView`. Its bounded attempt count scales with the target month,
  and a day is clicked only inside the safe viewport above the sticky footer. Do not replace
  this with translated label matching, a fixed swipe count, an unscoped screen scroll, or a
  bare `Displayed` check. These constraints are required for consistent local and
  BrowserStack behaviour and are the standard for human- and agent-authored tests.
- A recycled/missing calendar day does not imply forward scrolling. Compare the target with
  visible ISO day IDs inside the scroll viewport, and fail when direction cannot be observed.
  Derive the actionable vertical area from the scroll container and departure-summary footer,
  not a percentage of the device height. Keep the existing bounded gutter gesture to avoid
  activating day buttons during scrolling. TC05 and TC28 passed locally with this correction
  on 28 September 2026; date/enabled-state assertions were not relaxed.
- Shared iOS panel field readers locate the owning field by accessibility ID and read native
  static-text descendants with an iOS class-chain locator. Do not use Selenium `By.ClassName`
  on an `IWebElement`: it may translate to CSS, which is invalid in a native iOS context.
- New mobile page objects inherit `NativeAppPage`, which resolves keys by accessibility ID
  only. It deliberately does not fall back to generic XPath/text matching from the legacy
  cross-platform base page. A missing ID should fail quickly and visibly rather than spend
  time scanning a changing native tree.
- Where the app contract deliberately defines no ID, use its documented structural
  contract. PR 1040 defines the airport modal as containing exactly one displayed native
  `XCUIElementTypeSearchField`; do not locate it by its translated prompt text.
- Use explicit condition-based waits; never add `Thread.Sleep` for UI synchronisation.
- Do not tap fixed coordinates during normal journeys.
- Keep temporary XPath or text fallbacks inside page objects and document the app change
  and removal condition.
- `NativeFlightSearchPage` retains one such exception for the root `Book` tab: after an
  iOS app launch, BrowserStack can fail to resolve its visible `Book` accessibility
  ID even though the tab is present. It tries that ID first, then uses the proved semantic
  XPath fallback. Remove the fallback when the app exposes the tab reliably to the
  accessibility-ID lookup after launch.
- `TripsPage` applies the same primary-ID rule to `My Trips`, with a narrow iOS navigation
  predicate fallback after booking confirmation. The fallback covers the current case where
  iOS exposes the tab's icon/container metadata rather than its visible label. Remove it
  when the tab resolves reliably through its accessibility ID after the confirmation return.
- Native-control timeouts include the intended control and locator strategy. BrowserStack
  screenshots and source should then determine whether a failure is an app contract gap or
  a transitional screen state.
- Flight-result carrier selection reads one native page-source snapshot rather than issuing
  a remote Appium text/attribute call for every descendant of every result card. The legacy
  element-tree read remains only as a correctness fallback if page source is unavailable.
- Put shared startup behaviour in handlers. `AppEntryHandler` currently owns the Airship
  onboarding sequence, guest entry, and optional privacy-banner dismissal so individual
  journeys do not repeat it. Privacy handling prefers `Reject All` and falls back to
  `I Agree`, matching the established mobile framework.

The iOS onboarding carousel can expose off-screen duplicates. `OnboardingPage` resolves
the displayed and enabled accessibility-ID element inside the current viewport and waits
for each next carousel action before continuing.

Before Appium launches iOS, both local and BrowserStack drivers pass `processArguments` with
`uitesting`, the configured build profile, and any `IOS_AUTOMATION_FEATURE_FLAGS`. Search &
Book defaults include `850100-search-native-entry` and
`850101-search-native-skip-seat-selection`. The values are trimmed, deduplicated and rejected
unless they contain only safe flag-name characters. Tests do not navigate through the Debug
menu or restart the app to change feature state.

## Local and remote iOS support

### Optional controlled-failure pack

See [Sequential fault injection](../docs/MOBILE_FAULT_INJECTION.md) for the draft iOS
`Suite=FaultInjection` pack. It reuses Core's isolated proxy lifecycle, executes sequentially,
and requires explicit local-settings opt-in plus separately provisioned simulator routing/trust
or a dedicated BrowserStack Local external-proxy tunnel. Remote routing requires approval for
SIT session traffic through that tunnel; its configuration and cleanup are caller-owned.
It is not part of Sanity/Regression. TC01–TC05 passed on BrowserStack; TC06 passed locally,
but its BrowserStack UI/transport failure remains unconfirmed. Repeated-run and Android
qualification remain pending. TC07 passed locally against iOS PR1331 on 19 October;
TC08/09 remain offline-qualified only. See the test catalogue for individual seat-fault results.
Post-HPP cases require a separate payment opt-in; the external HPP itself is not intercepted.
No iOS code changes are included. Leave `FaultInjection.Enabled` false
for normal runs; the safe example contains the optional configuration keys.

### Supported hosts

The project and BrowserStack execution are supported from Windows, macOS, and Linux with
the .NET 10 SDK. Local iOS execution is macOS-only because Appium's XCUITest driver requires
Xcode and an iOS simulator or connected device. BrowserStack iOS execution does not require
Xcode locally and works from Windows, macOS or Linux. The setup script requires PowerShell
7 (`pwsh`) on each operating system.

Although the shared Appium driver contains Android support, the tests and selector contract
in this project currently target iOS. Treat Android as framework capability rather than a
supported test suite until Android-specific configuration, selectors and a connectivity run
have been validated.
# Card-presentation qualification

TC55 (`Suite=Regression`, `RegressionArea=SearchAndBook`) exercises LHR–LOS one-way in three days for one guest adult.
The app responds to create-order code `2001` after HPP; route/date alone is not an
app-side trigger. The test requires the verification page and instructions, agrees once,
then checks confirmation and Trips using the existing checkout flow. This creates a test
booking and requires explicit execution approval. It does not intercept responses or
retry payment. Cancellation is separate coverage, not implemented by this case.
Local qualification passed on 20 October through HPP, all four instruction paragraphs,
one agreement action, confirmation and Trips: `TestResults/tc55-los-local-20261020-c/tc55.trx`.
Test body 4m08.940s; execution including lifecycle approximately 4m48s. BrowserStack
qualification remains pending. Regression now contains 54 scenarios; Sanity remains 14.
