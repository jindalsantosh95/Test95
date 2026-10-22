# Pre-HPP Review total coverage

## Current figure-retention extension — 20 October

TC56 passed locally in 3m42s before this extension (one passed, no failures;
`TestResults/review-pr1342-20261020/review-pr1342.trx`, analyser clean).
It now additionally compares Flights, taxes/fees, Total, currencies and points individually
between the first Review visit and re-entry. Optional seat-row presence and charge are
also compared by the reusable helper. Arithmetic and Summary-total reconciliation run
on both visits. Offsetting fare/tax changes cannot pass merely because the total matches.
The extension is not yet live-qualified; TC56 itself remains a no-seat, pre-HPP journey.
No extra remote reads are needed: comparisons reuse the two existing price-card snapshots.

Validation: 591 offline/retry-policy cases passed, zero failed/skipped; exact-run analyser
reported no failures in `TestResults/review-figures-20261020/offline.trx`.
Automation `scripts/verify.ps1 -Base origin/main` passed build, offline checks and formatting.
The ten new offline cases cover retained GBP/USD/reward figures, row ordering, changed
currency/points/total, offsetting flight/tax amounts and changed/missing seat charges.

iOS PR1342 adds fare-destination IDs but those are not yet exercised by TC56. It deliberately
retains native Back (no `NativeFlightFareConditionsBackButton` was added). The older ID
proposal below is historical, not the delivered contract. No independent backend tax
calculation or fare-destination coverage is claimed by this figure-retention change.

## Agreed MVP scope and remaining functional contracts

Visual/design and accessibility audits are manually owned, not missing automated MVP work.
PWP eligibility/selection and ambiguous banner-configuration variants are outside this
ticket. Existing international-payment guidance checks remain in scope. Separate PWP tests
are retained rather than removed from the framework.

TC56 now implements Review back/re-entry without HPP: one adult, no seats, validate Review,
use `NativeFlightPaymentBackButton`, require passenger details, continue without refilling,
and revalidate retained price/currency and enabled payment action. It is Readiness-only
until locally qualified. It does not claim backend session renewal or field-by-field
passenger identity preservation.

Inspection of the current local iOS source still finds no itinerary block in
`FlightPaymentHandoffView`. Its fare link opens `NativeFlightFareConditionsView`, which
does show selected route/carrier/fare information but currently exposes no owned IDs.
Confirm whether this linked information is what the itinerary criterion refers to.
The functional checks cannot be completed robustly until that observation contract exists.
Proposed iOS-only accessibility additions (not implemented by this automation change):

- `NativeFlightFareConditionsPage` on the destination container.
- `NativeFlightFareConditionsBackButton` on its return action.
- `NativeFlightFareConditionsRoute{index}` with owned route/carrier/fare fields and
  stable route identity; expose expand/collapse actions and rule-content containers.
- If itinerary is instead directly on Review, provide its actual view and IDs rather
  than reusing Flight Summary as evidence of a different screen.

Then capture the selected itinerary before checkout, open the link, compare each offered
route/carrier/fare and available rules, return, and verify unchanged Review prices/readiness.
Do not assert illustrative Figma prices or use global English-text/positional selectors.
These items remain blocked, not silently skipped or counted as passing coverage.

## Latest local retry — 20 October 2026

TC43 retry passed seat selection and the shared pre-HPP assertions, then submitted the
test HPP payment. Observed Review breakdown: flights £514, taxes £399, seat £40, total
£953; the earlier Summary was £913. Reaching HPP proves the retained seat-detail/cart
comparison, Review arithmetic/currency, guest PWP exclusion and exact copy checks passed
for this single-adult GBP paid-seat journey. Other currencies/member journeys remain
unqualified for these new checks.

The overall test **failed** after 6m24s: the app displayed `NativeFlightPaymentErrorTitle`
(“We couldn’t confirm your booking”) and a message that payment/booking status could not
be confirmed. No payment retry was attempted and no confirmed PNR/Trips outcome obtained.
The existing happy-path error detector misses this dedicated screen, so the test eventually
reported the 180-second confirmation timeout. This fail-fast detection gap needs a separate
automation fix; the underlying payment/booking failure cause remains unconfirmed.

Follow-up: the shared happy-path fail-fast handler now recognises the dedicated error
title, message or page ID independently. Regression tests verify first-observation failure
and unchanged legacy/absent-error behaviour. This fix is offline-qualified, not a live
payment retest. Review checks remain embedded in existing complete-booking scenarios;
controlled-error and deferred PWP-selector flows deliberately use their separate paths.
Validation: 581 mobile offline/retry checks passed, zero failed/skipped; exact-run
analyser found no failures in `TestResults/payment-failfast-20261020/offline.trx`.

Evidence: `TestResults/review-seat-local-retry-20261020/tc43.trx`; exact-run analyser run.
This supersedes the earlier seat-map-blocked attempt below, not its recorded failure.

## Implementation — 20 October 2026

The shared `NativeBookingCheckoutFlow.Complete` path now captures the **Flight Summary**
price before passenger/seat entry and validates **Review total** before continuing to HPP.
Existing guest, US/USD, seat, redemption and fraud-check complete-booking callers reuse it.
No new full-booking scenario is added: Regression remains 54 and Sanity 14.

These assertions are implemented and offline-tested, **not yet qualified against the live
app**. Earlier passing bookings do not qualify these newly added checks. No iOS code changes,
backend requests, payment retries or new runtime settings are introduced.

| Requirement | Implemented assertion | Remaining limitation |
| --- | --- | --- |
| Price summary | One Flights, Taxes and Total row; optional seat row; exact decimal arithmetic | Does not independently verify backend tax components |
| Retention | Selected seat detail price equals seat-cart charge; cart total increases by that charge; Review seat row equals the retained selection price; Review total equals pre-seat Summary plus that charge | Single-adult GBP seat checkout; no independent backend seat-price comparison |
| Currency | Same GBP/USD/EUR symbol across Summary and breakdown | English display contract; unsupported formats fail explicitly |
| Reward | Flight points and total points equal the Summary points; cash taxes add correctly | Not Pay with Points apply/remove coverage |
| Guest | No Pay with Points card exists in the native hierarchy | Member eligibility/PWP selection remains separate deferred coverage |
| Guidance/actions | Exact English fare-link, international-payment guidance and Continue wording; enabled fare-conditions link and Continue | Link destination/back-navigation and banner-configuration assertions remain open |
| Itinerary/passengers on Review | Not asserted | Current `FlightPaymentHandoffView` has no itinerary/passenger summary; do not confuse earlier Summary with Review |
| Responsive/design/accessibility | Not asserted | Requires separate visual/accessibility qualification |

## App contract

- `NativeFlightPaymentSummaryCard` owns the price breakdown. iOS uses
  `.accessibilityElement(children: .combine)` on each row. Read accessible labels only
  within the single visible owned card; no global text locator, positional row or guessed ID.
- Row labels currently begin `Flights`, `Taxes, fees and charges`, `Seat selection`,
  and `Total to pay`. A missing, ambiguous or changed label fails, not silently passes.
- `NativeFlightSummaryPriceButton` exposes passenger context and total in its accessibility
  value; retain that value before checkout instead of deriving expected totals from Review.
- Use `NativeFlightPaymentFareConditionsLink`, `NativeFlightPaymentGuidance`,
  `NativeFlightContinueToPaymentButton` and `NativeFlightPayWithPointsCard` for controls.
- Dedicated IDs for each price row/value would improve the contract but are not invented
  by automation. Android requires equivalent owned containers/row semantics and qualification.

## Execution boundary

Validation waits for payment preparation to finish and fails immediately on recognised
payment errors. It runs **before clicking Continue to secure payment**. A failure therefore
does not submit an HPP payment. Controlled post-HPP fault-injection flows deliberately retain
their separate path; they do not inherit happy-path review checks. The deferred PWP selection
path is also unchanged.

Next qualification should cover one guest GBP journey, US/USD, redemption points plus cash,
and the seat journey. A passing price review alone is not a whole-ticket sign-off.

For the one-adult seat journey, capture the owned seat-detail price **before** confirming
the seat, then wait for the seat cart to show that charge and the expected trip total.
Carry this evidence into Review; never derive the expected seat charge from Review itself.
An explicit `Free` detail label means zero; missing/invalid evidence fails. A free seat may
omit the Review seat-charge row, but a paid seat may not. This reuses the existing GBP
seat-price contract and does not claim multi-passenger/multi-sector or non-GBP qualification.
Offline validation after this addition: 578 passed, zero failed/skipped, with exact-run
analysis at `TestResults/review-seat-offline-final-20261020/offline.trx`.
The full Automation `scripts/verify.ps1 -Base origin/main` harness also passed.

### Local qualification attempt — 20 October

TC43 ran on the iPhone 17 Pro/iOS 26.5 simulator, guest one adult, LHR–JFK one-way,
Virgin Economy Classic. Search and passenger entry completed, but the app exposed visible
`seatSelection.error` elements before a seat could be selected. The test failed after
1m55s without skipping seats, submitting payment or reaching Review total. Therefore none
of the new Review checks is live-qualified by this attempt. Backend cause is unconfirmed;
sanitised app logs contained no matching safe metadata.

Evidence: `TestResults/review-seat-local-20261020/tc43.trx`; the exact-run analyser reported
one unclassified failure requiring manual investigation. Native state:
`bin/Release/net10.0/TestResults/native-ui-failures/809e89dfb80e4409aa7bf9dcdb8e1e04/ui-state.xml`.

## Offline evidence

### Additional design checks

The wording checks use [the Review total Figma](https://www.figma.com/design/1Tt6zklO8HajnudMorkazx/06-Review--Payment-Page?node-id=30-11056).
They compare labels on app-owned controls, not text-based selectors. Only whitespace and
the native combined-label separator are normalized; missing or altered copy fails.
Five additional offline cases cover accepted separators and changed/incomplete labels.
Latest validation: **573 passed, 0 failed, 0 skipped**, including those five cases;
`TestResults/review-copy-20261020/offline.trx`. The exact-run analyser completed successfully.
No live app qualification was run for this addition.

Navigation and visual checks are **not implemented or counted as covered** yet:

- Fare conditions: the link has an ID, but `NativeFlightFareConditionsView` and its
  destination scroll view have no owned screen/content ID. Add a destination ID and
  explicit return action contract before asserting open/return/content preservation.
- Review back: `NativeFlightPaymentBackButton` exists, but `dismissPayment()` invalidates
  the payment session. This needs a dedicated cancellation/re-entry scenario, not an
  extra click in every complete-booking test. Verify the prior page, retained booking
  selections and a newly prepared payment session before resuming.
- Visual: no approved Review-page reference image is in the baseline manifest. Agree
  a pinned-device Figma export, component bounds and masks for dynamic prices first.
  Do not turn an app screenshot into its own expected design or compare illustrative
  Figma prices with real totals. No iOS implementation changes were made.

Release build and 568 mobile offline/retry-policy checks passed (15 new Review total cases).
Run: `TestResults/review-total-offline-final-20261020/offline.trx`; exact-run analyser
reported no failures. The full Automation harness also passed solution build and
architecture/Core offline checks. These results do not establish native row accessibility
or live price retention; that requires the targeted local qualification above.
