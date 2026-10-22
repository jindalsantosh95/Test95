# Native Search and Book — seats coverage and app contract

## Pending requirements — US-origin Economy Delight free seats

**Status: coverage gap; awaiting PO/developer requirements. Not implemented or qualified.**
Raised on 20 October 2026. The expected entitlement has been requested for clarification;
do not yet treat all Economy Delight seats as free or encode an assumed eligibility rule.

Proposed representative journey: JFK–LHR, Virgin-operated, Economy Delight, USD.
Once requirements are confirmed, verify the eligible seat's explicit free/zero price before
selection, retained assignment, unchanged booking total, and no seat charge (or an explicit
zero row) on Review. Confirm whether booking completion and post-booking seat assignment
are required as part of acceptance.

Requirements to confirm:

- Eligible seat categories and exclusions (including preferred/exit-row seats).
- Operating/marketing carrier, codeshare and sector/connection eligibility.
- Any passenger, loyalty-status, booking-channel or timing restrictions.
- Expected free-price wording and Review row behaviour.

TC23 uses Economy Classic and proves USD booking behaviour, not this entitlement.
The existing seat-price evidence helper is GBP-qualified; USD support and live qualification
will be needed for this scenario. No test ID or release-pack membership is allocated yet.
Exclude this gap from implemented/passing totals and do not increase component coverage
on the strength of this plan. Revisit during coverage review when requirements arrive.

Release selection (16 October): TC44–54 belong to Regression; TC47 and TC48 also
belong to Sanity. TC51–54 intentionally expose the current child-guidance accessibility
failure in release results. TC43 remains qualification-only due to payment preparation issue #177.
See [release packs](RELEASE-PACKS.md) for filters and sign-off exclusions. Enrollment
does not establish repeated-run or BrowserStack stability, nor paid-seat fulfilment.

Start with the [current test-status table](TEST-CATALOG.md#current-status--12-october-2026).
The dated notes below retain the investigation history; later results supersede earlier failures
only for the scenario and assertions actually rerun.

9 October rerun: TC50 passed in 4m51s, proving unsupported-carrier seat bypass and
the same-session Virgin seat-enabled control. TC43 failed at payment preparation Retry
after its seat flow (2m52s). TC48 failed earlier at fare selection (1m01s); its Trips
matching fix was not reached. No payment submitted. Exact evidence:
`TestResults/mobile-booking-recheck-final-20261009/local.trx` (one pass, two failures).
All remain Qualification; issue #179 is not closed by one successful attempt.

Later 9 October standalone TC48 passed payment, confirmation and Trips in 3m27s after
automation-only HPP input hardening. Evidence: `TestResults/tc48-hpp-hardened-20261009/local.trx`.
This supersedes TC48's earlier failure, not TC43's seat-payment issue. No remote qualification.

## Family and carrier expansion — 8 October 2026

See [the source-grounded implementation plan](SEATS-FAMILY-ELIGIBILITY-PLAN.md) for child
proximity, lap infants, infant restrictions, VS/DL eligibility, unsupported operators and
mixed/codeshare sectors. These are planned scenarios, not additional executable coverage.
The current iOS code filters infants and limits carrier/fare eligibility. The user confirmed
child proximity is warning/information only, with no hard stop. Planned tests must verify
the advisory and continued ability to proceed; exact trigger/content/age scope remain pending.

Latest 7 October follow-up: TC44 passed in 2m55s. TC43 repeatedly reached payment preparation
but showed Retry; tracked in [delivery bug #177](https://github.com/VAAD1ST/vaa-mobile-delivery-artifacts/issues/177).
TC48 without seats reached payment and confirmation, then falsely failed its Trips lookup:
the matching booking is visible in the failure screenshot. It is not an overall test pass.
TC46 covers two adults, not child proximity or infants; TC47 covers paid draft charge
replacement/removal, not free seats, breakdown rows or completed seat fulfilment.
Historical statements below must be read with these later qualifications.

## Latest implementation — 7 October 2026

### Passenger readiness investigation

The remaining-pack run passed TC46 but failed TC44/45 on their first passenger assignment read.
The seat heading can appear before the map/passenger controls. TC46/47 waited for the active
passenger; TC44/45 did not. Shared `SeatScenarioTestBase` now waits for active passenger 0 before
invoking any seat-only scenario. It uses the existing bounded observable wait and immediate
app-error/Payment detection, with no sleeps, retries or fallback selectors. Build and 369 offline
checks passed. Targeted rerun: TC45 passed; TC44 now correctly detected an explicit seat-map
error/unavailable state while waiting, rather than throwing the premature missing-element error.
TC44's assignment outcome remains blocked; the new failure is not conclusively correlated to
the earlier downstream error. Exact-run analyser completed for
`TestResults/seats-readiness-local-20261007/local.trx` (one pass, one failure).
No iOS change. Wider verifier remained incomplete at its stalled formatter, which was stopped.

TC43 reached payment preparation and failed because the app displayed Retry. The automation
correctly stopped without retry/payment submission. That failure is separate from passenger
readiness and needs app/backend investigation, not an automation bypass. Evidence:
`TestResults/seats-remaining-local-20261007/booking.trx` (UTC 14:49:49–14:52:02).

### Snapshot performance follow-up

TC47 passed in `TestResults/seats-tc47-downstream-recheck-20261007/local.trx` after
the downstream error investigation: 343.51s test body, approximately 359s including lifecycle.
The three discovery snapshots took 35.21s, 35.90s and 57.15s (128.26s combined).
Fresh pre-tap safety took approximately 11s per interaction.

Automation-only optimisation: paid-seat discovery now uses per-call `mobile: source` XML with
`excludedAttributes=visible,accessible`, supported by the installed XCUITest driver. Snapshot
geometry, owned identifiers, enabled state and advertised prices identify candidates only.
Every tap still requires fresh live visibility, enabled state, full safe-area containment and
native hittability. No global driver settings, cached safety geometry, relaxed assertions or
iOS changes. The maximum three measured drags and immediate app-error failure remain unchanged.
See [Appium's source command](https://appium.github.io/appium-xcuitest-driver/10.43/reference/execute-methods/#mobile-source).

Build and 369 offline/retry checks passed. TC47 passed locally in
`TestResults/seats-tc47-source-speed-local-20261007/local.trx`: 204.48s test body,
approximately 212s including lifecycle, versus 343.51s/359s before (about 41% faster overall).
Discovery snapshots took 0.55s, 0.57s and 0.56s: 1.68s combined versus 128.26s.
The same 46D GBP 40 -> 50A GBP 35 -> removal assertions passed, with one measured drag and
totals GBP 964.20 -> 959.20 -> 924.20. Fresh safety checks remained approximately 9.4s each.
Exact-run analysis found no failures. No submission/payment. This is one local comparison,
not repeated-run or BrowserStack qualification. Wider verification remains incomplete because
its restore-dependent formatter stalled and was stopped.

### Earlier timing/consolidation follow-up

Automation-only: combined Payment/error detection into one ID predicate query, removed the duplicate
candidate safety check (fresh pre-tap validation retained), and replaced nested price waits with one
outer observable wait. Added finally-recorded timing for discovery geometry/snapshot, seat lookup,
fresh pre-tap safety, tap, detail readiness, bounded scroll and price assertions. No iOS change.
364 offline/retry checks passed; changed file formatted. Wider harness remains incomplete due to
stale review records and its stopped restore-dependent formatter.

Latest local attempt: FAILED before seat discovery, with the app's seat-map error/unavailable state
(1m42s; runner 1m57s). Startup/search/summary 54.19s; passenger/seat entry 35.23s.
`TestResults/seats-tc47-profile-local-20261007/local.trx`; exact-run analysis completed. Safe app logs
did not establish the backend cause. No retry/skip/payment. This is not a performance result and does
not qualify the optimised version; the 6m02s pass below belongs to the preceding implementation.

**Latest TC47 result: PASS, 6m02s** (6m09s runner duration). Evidence:
`TestResults/seats-tc47-safe-area-local-20261007/local.trx`; exact-run analyser found no failures.
Selected 46D at GBP 40, total GBP 964.20; one safe-area drag; replaced with 50A at GBP 35,
total GBP 959.20; removed 50A, restoring GBP 0 charge and GBP 924.20 total. Both map/detail
prices and exact assignments were checked. No seat submission/payment. Automation-only fix;
current iOS PR1273 viewport ID is sufficient for this run. Historical blockers below are superseded
for TC47, not proof of repeated-run stability or BrowserStack qualification. Remains Qualification.
364 offline/retry checks and 10 architecture checks passed. Wider verification remains incomplete
with stale harness reviews and a stopped restore-dependent formatter. Changed files were formatted.
Execution remains slow; do not claim a substantial speed improvement from this single pass.

### Automation-only safe-area refinement

iOS PR1273 commit `79e040a3` now exposes the viewport and the local test found it. The first
hardened run passed the GBP 40 seat check but rejected replacement scrolling because the full
native viewport failed the strict header/cart bounds check (6m13s). Fifteen offscreen candidate
lookups also made discovery slow. Evidence: `TestResults/seats-tc47-viewport-local-20261007/local.trx`.

The automation now intersects the owned viewport with the window, header bottom and a conservative
cart boundary (one observed charge-row height above that row). It filters snapshot seat rectangles
in memory before remote lookups, capped at eight in-bounds candidates per viewport. Native drags
use endpoints at 75%/25% of that measured safe area, at most three, rather than swiping the entire
reported scroll view. No hard-coded screen coordinates or coordinate seat taps. Exact-ID seat taps
still revalidate safe geometry and native hittability. No iOS change or rebuild for this refinement.
Build and 364 offline/retry checks passed; live TC47 qualification pending.

### Seat interaction hardening (runtime qualification pending)

The rebuilt TC47 attempt verified the first GBP 40 seat and GBP 964.20 total from a GBP 924.20
baseline, then unexpectedly reached Payment while opening a replacement. Evidence:
`TestResults/seats-tc47-local-rebuilt-20261007/local.trx` (3m41s). The failure hierarchy contains
`NativeFlightPaymentHeading` and `NativeFlightPaymentRetryButton`. The exact replacement was not
logged; an obscured-seat tap hitting the fixed cart is a hypothesis, not a confirmed cause.
No payment completion is evidenced; whether seats were submitted or skipped is unknown.

The shared seat page now requires native `hittable`, full seat containment in the owned vertical
viewport, and bounds clear of navigation/cart before tapping. It records candidate IDs, prices,
bounds and rejection reasons, never passenger labels. Paid-seat discovery permits at most three
native upward swipes inside the uniquely identified native scroll viewport. No coordinate taps,
blind swipes or automatic journey retries. Unexpected Payment navigation fails during action waits.

**Required iOS change (not implemented here):** add `seatSelection.viewport` to the outer vertical
`ScrollView` in `NativeFlightSeatSelectionView`, not `seatSelection.map`'s full grid content. The
exposed bounds must be the clipped viewport above the fixed cart. Validate the accessibility node
is a unique `XCUIElementTypeScrollView` and does not propagate its ID to children. Until this is
available, shared seat interactions fail before tapping. TC43–TC47 require runtime requalification;
historical passes below do not qualify this hardened version. No iOS source change/push this turn.
Validation: Release build and 363 offline/retry checks passed in
`TestResults/seat-safety-offline-20261007/offline.trx`; 10 architecture checks passed in
`TestResults/seat-safety-architecture-20261007/architecture.trx`. Exact-run analysis found no failures.
Changed files were formatted. Wider harness still flags stale context-routing/registry reviews.

TC47 implements paid-seat price recalculation; local qualification is blocked by the installed app contract. Maps to
`displayedSeatPrice`, `displayedTotalPrice`, draft replacement/removal and cart/detail presentation.
It reads the three PR1273 price IDs, requires a zero initial charge and positive trip total,
discovers two visible positive GBP offers of different prices, matches each detail price to its map
offer, and verifies replacement does not double-count; removal restores the baseline.
One in-memory map snapshot per discovery avoids remote per-seat enumeration. Exact IDs are
revalidated for interaction; no offscreen taps, guessed seats or blind scrolling. Insufficient
visible priced inventory is an explicit failure, not a pass/skip. GBP/English only; no payment.

Local attempt on 7 October failed in 2m44s at the first `seatSelection.seatPrice` lookup.
Search/summary took 55.70s; passenger entry/submission/seat screen took 35.77s. The cart visibly
showed zero charge and a GBP 924.20 total, but neither cart-price ID appeared in the sanitised
accessibility snapshot. No seat-price selection/replacement/removal assertion was exercised.
The installed debug binary timestamp was 09:47 BST, before PR1273 commit `c12c922d` at 11:48 BST;
pull/rebuild/install that commit before retrying. This is not evidence of a price-calculation defect.
Evidence: `TestResults/seats-tc47-local-20261007/local.trx`. Exact-TRX analysis completed; its generic
service-availability inference is not supported by the captured missing-ID evidence.
Release build and 362 offline/retry checks passed; 10 architecture checks passed. Scoped no-restore
format verification passed. Wider verification remains incomplete: stale context-routing and
operational-registry reviews, plus its stalled restore-dependent formatter, which was stopped.
BrowserStack and repeated-run stability remain unqualified.

TC46 adds two-adult independent assignments (local rerun passed in 2m37s). It maps to
`NativeFlightSeatSelectionViewModel.select`, `deselect`, `isSelectable` and the passenger carousel:
automatic advance to the unassigned traveller; assigned seats disabled for the other traveller;
switching preserves both assignments; removing the first does not change the second.
Existing synthetic guest setup and bounded ID-only seat interactions are reused. This is a separate
`Suite=Qualification` class, stopping before seat submission/payment. No iOS change is required.

First local run failed in 2m09s: search/summary took 50.22s and two-passenger entry/submission
took 47.90s. The seat screen then exposed `seatSelection.error` with no passenger cards;
the original initial wait timed out after 30s. Evidence:
`TestResults/seats-tc46-local-20261007/local.trx`; the sanitised hierarchy confirms the app error.
No safe syslog metadata identified the backend cause. The error does not establish a
multi-passenger product defect or a missing passenger ID. Initial active-passenger and unavailable-seat
waits now check terminal app errors without retrying; this hardening is offline-validated only.
The user-authorised rerun passed on 7 October: `TestResults/seats-tc46-local-rerun-20261007/local.trx`.
All assignment assertions executed, including native selected-state auto-advance and disabled-seat
exclusivity. Search/summary took 53.65s, passenger entry/submission 47.54s, offered-seat opening
4.61s/4.65s and confirmation 3.05s/3.10s. Exact-TRX analysis found no failures. No payment.
This supersedes the unqualified result above, but does not establish the earlier error's cause
or repeated-run stability. No code changes were needed for this rerun; BrowserStack remains pending.
Validation: Release build and 349 offline/retry checks passed
(`TestResults/seats-tc46-final-offline-20261007/offline.trx`); 10 architecture checks passed
(`TestResults/seats-tc46-architecture-20261007/architecture.trx`). Exact-TRX analysis completed
for all runs. The generic live analyser inferred service availability; the captured app error
is confirmed, but its backend cause is not. Wider harness verification is not green: stale
context-routing/registry reviews remain, and its stalled restore-dependent formatting step was
stopped. Changed-file no-restore formatting is checked separately.

This update supersedes the historical 6 October blocker below. iOS PR1273's container repair
allowed TC43 to select/assign a seat and submit through to Payment. Two runs then failed at
payment preparation (Retry); no payment submission or confirmed booking. Exact latest evidence:
`TestResults/seats-pr1273-payment-retry-20261007/local.trx`.

Two independent pre-payment tests now isolate UI seat behaviour:

- TC44: select, replace and remove; verify exact assignment and restoration of unassigned state.
- TC45: close details/key before and after selection; preserve the current assignment.

Both are `Suite=Qualification`. Before the optimisation, TC44 passed locally in 3m18s and TC45 in 3m27s.
TC44 evidence: `TestResults/seat-lifecycle-narrow-local-20261007/local.trx`. Opening each offered
seat/detail took 43.22s/44.11s; confirming assignment took 3.81s/3.70s. These timings include
automation command overhead and are not pure backend or app-render measurements.
TC45's detailed timings (`TestResults/seat-sheets-local-20261007/local.trx`) isolate 39.53s/39.01s
to retrieving 127 enabled seat elements. Visibility took 0.50s/1.59s, tap about 1.1s and detail
readiness about 1.3s. Discovery now requests one native candidate at a time, excludes the current
seat when replacing, and inspects at most eight candidates. Off-screen candidates are not tapped.
If none is visible within that limit, it fails with the missing owned-scroll requirement instead
of swiping blindly. TC43 reuses this helper.

**Final optimised run:** both passed on the local iPhone 17 Pro / iOS 26.5, TC44 in 1m57s and
TC45 in 2m15s. Candidate lookup/visibility took 1.65-1.67s; discovery/tap/detail readiness took
4.57-4.61s, compared with 43-44s before. Confirmation/assignment remained about 3.6-3.7s.
Evidence: `TestResults/seat-focused-final-20261007/final.trx` (351 passed: 349 offline plus two live;
4m36s including serialized sessions). Exact-TRX analysis found no failures. Ten architecture tests
passed separately (`TestResults/seat-architecture-restored-20261007/architecture.trx`), and scoped
no-restore formatting passed. The wider `verify.ps1` remains blocked by stale context-routing and
operational-registry review metadata, plus its restore-dependent format step; these are not a green
whole-repository gate. No iOS change, payment, BrowserStack run or full TC43 rerun in this update.
Each owns an isolated Appium session;
local execution serializes and BrowserStack can parallelize classes. The shared setup reuses
guest search, exact Economy Classic selection and synthetic passenger entry, and requires seats
instead of silently accepting a payment bypass. Both require two visible enabled offers and fail
explicitly if inventory is insufficient. No unscoped scrolling, payment retries or fixed sleeps.
The page narrows lookups to owned IDs before checking individual visibility and uses one combined
error-ID lookup. A first local attempt using `visible` in native predicates stalled on the seat map:
the app consumed roughly one CPU core, and a stack sample showed accessibility visibility/hit-test
work. That attempt was cancelled without a final TRX; it is not a completed failure or pass.
The broad visibility predicate was removed before retrying. Timings separate setup/passenger work
from detail opening and assignment. This is an optimisation candidate, not measured speedup.
English `Choose a seat` and `Remove seat` are state assertions on ID-addressed controls, not locators.

Still not covered by these tests: detailed seat price/cabin/characteristics, totals/breakdown,
unavailable-seat interaction, multi-passenger/flight assignment, retained selection after navigation,
exit restrictions, controlled failures and authoritative post-booking fulfilment. Missing value and
scroll IDs below remain app-owned readiness work; no iOS source change is included here.

Run just these checks with `Journey=Seats&TestType=FunctionalValidation` and run-scoped TRX.
Their per-session launch override omits the skip flag; an existing saved developer skip override
must also be off. The tests never change global settings or developer preferences themselves.

Reviewed 6 October 2026 against iOS develop
[`f63db4fb`](https://github.com/VAAD1ST/vaa-mobile-ios/tree/f63db4fbfe80cf8793de6a5e392db5e35d7a61d6).
This is a behavioural mapping, **not measured Swift line/branch coverage**.
Existing iOS unit tests are not counted as Appium coverage.

## Historical position — 6 October 2026, superseded by the update above

Only TC43 is implemented as a seats journey. It is in `Suite=Qualification`, not regression,
and has **not passed**. It searches as one guest adult, selects Virgin Economy Classic,
reuses passenger entry, requires seat selection, verifies a passenger's selected seat, then
intends to complete payment, confirmation and booking presence in Trips.

Observed on the installed iPhone 17 Pro/iOS 26.5 build: search, Classic selection, passenger
entry, seat-map loading and tapping an offered seat work. Seat-detail confirmation is blocked
by the accessibility lookup. Seat assignment, seat submission, payment and post-booking seat
retention have not been qualified by this test. The existing guest smoke passing without seats
does not establish seat coverage.

The installed binary contains `seatSelection.selectSeat`, `seatSelection.detail.close` and
`seatSelection.detailSheet`. Source presence is not proof that XCTest exposes these distinctly.
The detail sheet applies an outer ID without an explicit accessibility container boundary.
**Runtime investigation confirmed the collision:** two visible buttons have the ID
`seatSelection.detailSheet`, while `seatSelection.selectSeat` cannot be located. The screenshot
shows both Select seat and Close, so this is not a missing/slowly rendered button.

Diagnostic evidence: `TestResults/seats-contract-diagnostic-20261006/local.trx`:
344 offline checks passed; one diagnostic TC43 variant failed at this contract in 4m17s total.
The variant had an explicit stop before HPP even if selection succeeded; that temporary source
guard was removed afterwards. No seat confirmation, seat submission or payment was performed.
Sanitised UI evidence:
`bin/Release/net8.0/TestResults/native-ui-failures/d325f1b3d3924e178fcf87051993b9da/ui-state.xml`.
The report preserves exact known IDs and states but omits passenger labels/values.

Expected accessibility tree: one separately identifiable detail container, one visible enabled
confirm button with `seatSelection.selectSeat`, and one close button with
`seatSelection.detail.close`. Current tree: both action buttons inherit the container ID.
This establishes an app accessibility-contract defect, not a backend failure or justification
for coordinate/first-button selection.

## User-facing coverage matrix

“New” means required/proposed coverage, not an executable or passing test. “Amend” means extend
TC43 after its blocking contract is fixed. Keep each independently runnable scenario in its own
class; share search/passenger mechanics. Only the happy path needs a completed payment.

| User behaviour | iOS implementation | Required observable assertion | Coverage / intended pack |
| --- | --- | --- | --- |
| Enter seats only when eligible | `FlightPassengerDetailsViewModel.submit`, `FlightBookingSeatSector.isSeatMapEligible` | Classic eligible VS/DL sector opens seats; Light and unsupported operators bypass; skip flag tested separately | TC43 entry observed; explicit bypass cases New |
| Route, current flight and cabin | `NativeFlightSeatSelectionHeader`, `ViewModel+State`, `SeatMapGrid` | Header agrees with selected itinerary and sector; correct flight position/count and cabin | New: multi-flight navigation |
| Seat map and unavailable seats | `NativeFlightSeatMapPresentation`, `SeatMapControls`, `isSelectable` | Selectable seats have an offer for active traveller; disabled seats cannot open selection; map can be traversed safely | TC43 discovers an enabled seat only; broader availability New |
| Inspect and cancel without selecting | `SeatMapDetailSheet`, view's `inspect` / `onDismiss` | Number, fare/cabin, position, price and supplied characteristics/restrictions describe tapped seat; Close leaves assignment/count/total unchanged | TC45 verifies assignment preservation; content/count/total remain gaps |
| Select, replace and remove seat | view's `onConfirm`, view model `select` / `deselect` | Confirmation assigns exact seat; replacement releases old seat; reopening selected seat offers removal; removing restores unselected state and price | TC44 verifies exact assignment, replacement and removal; released-map state and price remain gaps |
| Exit-row acknowledgement | `confirmTitle(for:)`, `SeatMapDetailContent.noticeView` | Exit notice shown and explicit acceptance required; Close does not assign; accepted seat is assigned | New: exit-row scenario, needs eligible inventory |
| Passenger switching | `PassengerCarousel`, `ViewModel+State.travellers`, `select` | Correct active passenger; automatic advance to next unassigned offered passenger; assignments retained on switching; same seat unavailable to another passenger; infant not given a separate seat card | New: two eligible passengers plus infant; synthetic traveller data |
| Free/paid prices and breakdown | `ViewModel+State.pendingSeatPriceAdjustment`, `Cart`, price-summary sheet | Count, seat delta, trip total and breakdown agree; free adds zero; replacement/removal use differences, not duplicate charges; currency consistent | New: draft lifecycle; Amend TC43 price retention |
| Seat key | `SeatMapLegend`, `legendEntries` / `legendFacilities` | Key opens/closes without changing selection; free/paid/exit/occupied/blocked/selected and facilities have expected descriptions | TC45 verifies open/close and assignment retention; descriptions remain gaps |
| Continue across flights / go back | `continueFromCurrentSector`, `advanceFromCurrentSector` | Next flight changes route/progress; previous selections persist; final eligible flight offers Confirm and pay; no duplicate charges when revisiting unchanged selections | New: return or connecting journey; one smoke combination rather than every route |
| Continue without a selection / Skip to payment | `skipCurrentSector`, `skipToPayment` | No-selection Continue advances only current sector; Skip goes to payment; unsubmitted choices must not be reported as purchased seats | New: skip scenario; confirm draft/confirmed pricing expectations with product before asserting ambiguous behaviour |
| Loading, empty map and retrieval failure | `loadSeatMap`, `applyResolvedSeatMap`, `retrySeatMap` | Loading resolves; no selectable offers skips that sector; error exposes Retry; one explicit retry recovers without losing prior selections | New: controlled backend-data/failure fixture required; never rely on random service outages |
| Submission failure / authoritative refresh | `continueToPayment`, seat service reconciliation | Remain on seats with error, no payment; totals follow refreshed basket rather than unconfirmed draft; successful retry does not duplicate charges | New: controlled failure fixture; no automatic mutation retry in generic harness |
| Completed booking | confirmation view, existing HPP/Trips flow, seat service reconciliation | Booking confirmed, total includes confirmed seats; exact seat per traveller/sector retained in authoritative booking/Trips | TC43 intended payment path blocked; Amend with backend/Trips assertion before claiming seat fulfilment |

## Lean scenario split

1. **TC43 happy path:** one eligible adult, offered seat, price reconciliation, test payment,
   confirmation and Trips. Add authoritative seat-retention verification; currently only booking
   presence in Trips is asserted.
2. **Details/key/draft lifecycle:** inspect, close, key, select, replace, remove and price breakdown.
   Stop before submitting seats/payment. Use free and paid offers when deterministic data permits.
3. **Passenger assignment:** two seat-eligible passengers and an infant; switching, automatic advance,
   duplicate-seat prevention and retained assignments. Stop before payment.
4. **Multiple flights:** representative return/connection; route/progress, next/back and retained
   selections/prices. Submit seats if required, but do not repeat full HPP.
5. **Skip:** distinguish skipping one sector from the remaining journey; assert the payment summary,
   then stop before payment.
6. **Eligibility:** explicit Light/unsupported-operator bypass, eligible Classic entry and the developer
   skip override. Keep flag variants isolated; do not silently switch to a different fare.
7. **Exit row:** supplied restrictions, cancel, explicit acknowledgement and assignment.
8. **Controlled recovery:** retrieval and submission failures are separate deterministic scenarios,
   deferred until a supported environment/data contract exists.

Apart from TC43 and the assignment/cancellation portions now owned by TC44/TC45, these are proposed
scenario groups, not allocated TC numbers. Implement one class per concrete scenario and register
it in the JSON contract and test catalogue when its app/data contract is available.
Do not inflate the regression inventory with unimplemented placeholders.

## Accessibility readiness work

### Existing actions that must remain distinct at runtime

- Detail: `seatSelection.detailSheet` container, `seatSelection.selectSeat` confirm,
  `seatSelection.detail.close` close.
- Key: `seatSelection.legend`, `seatSelection.legendSheet`, `seatSelection.legend.close`.
- Price summary: `seatSelection.total` is the **View breakdown button**, not the total-value label;
  `seatSelection.priceSummary` and `seatSelection.priceSummary.close` are separate.
- Map/passengers: `seatSelection.seat.<number>` and `seatSelection.passenger.<index>`.
- Continue/skip/errors: `seatSelection.continue`, `seatSelection.skip`,
  `seatSelection.error`, `seatSelection.retry`, `seatSelection.submissionError`.

The detail-sheet repair in iOS PR1273 establishes an explicit SwiftUI accessibility container
(`children: .contain`) at the owning sheet root, with its container ID, while preserving the
existing independent button IDs. Validate in XCTest/Appium before accepting this as a fix.
Do not combine or hide the action children; do not change handlers, prices, seat logic or layout.
The legend sheet has the same repair; TC45 qualifies its close action. No iOS source is changed
in this automation update. VoiceOver grouping still needs manual app-side qualification.

### Missing observation/scroll contracts in reviewed source

Suggested IDs (proposals, not delivered app identifiers):

- Owned vertical viewport: `seatSelection.scrollView`, separate from the grid's `seatSelection.map`,
  for safe bounded scrolling on long seat maps.
- Cart value elements: `seatSelection.selectedCount`, `seatSelection.seatPrice`,
  `seatSelection.tripTotal`. Current count/delta are combined text, without dedicated IDs.
- Detail values: `seatSelection.detail.seatNumber`, `.cabin`, `.seatType`, `.price`,
  `.characteristics`, `.restrictions`, `.exitNotice` as applicable.
- Key entries/facilities: stable semantic IDs such as `seatSelection.legend.entry.available`;
  avoid locating by row order, icons or coordinates.

Expose existing user-visible values only. No test-only backend metadata or booking IDs should be
added to the UI for this work. Inherit equivalent semantics in Android; do not claim Android
qualification from iOS source inspection.

## Backend reuse and confidence

Existing EAPI Trips `SeatMapTests` exercise VS/DL seat-map APIs. Reuse their request/model/core
capabilities for targeted independent backend assertions rather than copying clients into mobile.
Their API passes do not prove these UI interactions. A post-booking seat-fulfilment check needs an
authorized booking lookup and exact traveller/sector association; a confirmed PNR alone is insufficient.

The app already reconciles submitted/removed/retained selections and basket amounts in
`SearchAndBookFlightSeatServiceAdapter+Reconciliation`. Treat that as implementation to exercise,
not independent evidence that the Appium booking retained its seat.

Source review confidence is high for the behaviours above at the pinned revision. Runtime
qualification remains partial and does not establish behaviour on all carriers, cabins, layouts,
devices, Android or BrowserStack. The installed binary's exact source SHA is not established merely
by finding its identifier strings.

## Source entry points

- [Seat screen, sheets and actions](https://github.com/VAAD1ST/vaa-mobile-ios/blob/f63db4fbfe80cf8793de6a5e392db5e35d7a61d6/Modules/vaa-mobile-ios-search-and-book/Sources/SearchAndBook/UI/NativeSeatSelection/NativeFlightSeatSelectionView.swift)
- [Seat-map load, selection, submission and navigation](https://github.com/VAAD1ST/vaa-mobile-ios/blob/f63db4fbfe80cf8793de6a5e392db5e35d7a61d6/Modules/vaa-mobile-ios-search-and-book/Sources/SearchAndBook/UI/NativeSeatSelection/NativeFlightSeatSelectionViewModel.swift)
- [Passenger filtering and displayed price calculation](https://github.com/VAAD1ST/vaa-mobile-ios/blob/f63db4fbfe80cf8793de6a5e392db5e35d7a61d6/Modules/vaa-mobile-ios-search-and-book/Sources/SearchAndBook/UI/NativeSeatSelection/NativeFlightSeatSelectionViewModel%2BState.swift)
- [Reusable detail sheet and independent button IDs](https://github.com/VAAD1ST/vaa-mobile-ios/blob/f63db4fbfe80cf8793de6a5e392db5e35d7a61d6/Modules/vaa-mobile-ios-seats/Sources/SeatMap/SeatMapDetailSheet.swift)
- [Seat-sector eligibility](https://github.com/VAAD1ST/vaa-mobile-ios/blob/f63db4fbfe80cf8793de6a5e392db5e35d7a61d6/Modules/vaa-mobile-ios-search-and-book/Sources/SearchAndBook/Models/FlightBooking/FlightBookingBasketContext.swift)
## TC49 lap-infant qualification — 8 October 2026

`SearchBook_TC49_AdultWithLapInfant_OnlyAdultHasIndependentSeatAssignment` historically passed
locally on iPhone 17 Pro / iOS 26.5 in 4m08s (4m27s including lifecycle).
One adult plus one lap infant exposes count one through the PR1273 accessibility
value; offered adult seat selection, replacement and removal pass, with passenger
zero remaining active. No seat submission or payment. Infant restrictions,
BrowserStack and repeated-run stability remain unqualified. Suite=Qualification.
Evidence: `TestResults/lap-infant-20261008/local.trx`; analyser found no failures.

iOS #1311 (`8c43bf64`) now exposes the linked infant on the adult passenger card as
`<name> + infant`. TC49 requires that text through the existing
`seatSelection.passenger.0` identifier while retaining the complete count-one assertion.
This added assertion has not been run against a build containing #1311, so the 8 October
evidence does not qualify it.

iOS #1318 (`bbd75f93`) delivers `seatSelection.childGuidanceSheet` for bookings containing a
child on the first eligible sector. Its mandatory Choose seats button has no app-owned
accessibility identifier. A new child scenario is therefore blocked by the selector contract;
do not substitute English text, XPath or coordinates. The confirmation W003 warning is a
separate post-payment contract and needs deterministic backend data.

TC51–54 now cover display without dismissing the sheet: eligible Virgin, unsupported outbound
followed by eligible Virgin inbound, all-unsupported bypass, and direct DL eligibility. They are
Qualification-only and have no live evidence. The stable sheet ID proves the messaging view is
displayed; exact title/body assertions still need IDs such as `seatSelection.childGuidance.title`,
`.supervision` and `.position`. The Choose seats action ID is only required to continue beyond
this display boundary. A genuinely eligible-but-unavailable outbound map followed by an eligible
inbound remains a deterministic-data gap; it is not equivalent to a carrier-ineligible outbound.
375 offline checks passed. The wider harness failed on context-routing,
operational-harness and format restore; scoped no-restore formatting passed.
## TC50 unsupported operating-carrier bypass — qualification pending

`SearchBook_TC50_UnsupportedOperatingCarriers_BypassSeatsWhileVirginRemainsEligible`
is implemented in an isolated qualification class. Read all operating legs and accept
explicit AF/KL only, select partner Economy Standard, verify Summary retains the fare,
then require direct payment entry without seat Continue/Skip. A same-session Virgin
Classic journey must still open seats, preventing a saved global skip flag false pass.
Neither journey submits seats or pays. This is representative AF/KL coverage, not
all unsupported airlines or mixed eligibility.

Local 8 October attempt stopped before Summary with the application's fare-selection
error: "We could not start this search just now. Please try again." Standard was selected
and shown for both KLM LHR-AMS-JFK legs. No backend root cause established. Bypass and
positive control remain unexercised; BrowserStack pending. Exact-run evidence:
`TestResults/seat-bypass-20261008/local-standard.trx` (2m25s). 391 offline checks passed.
The first run exposed a Classic-vs-Standard automation assumption, now corrected.
Wider harness failed context-routing/operational-harness; formatting passed.

The requested repeat reproduced the same error before Summary (2m34s):
`TestResults/seat-bypass-20261008-rerun/local.trx`.
Tracked in [iOS bug #179](https://github.com/VAAD1ST/vaa-mobile-delivery-artifacts/issues/179).
The error is observable in the app; a backend root cause has not been established.
