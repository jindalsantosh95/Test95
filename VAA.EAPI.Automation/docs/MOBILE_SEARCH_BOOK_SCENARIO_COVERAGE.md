# Mobile Search and Book scenario coverage

## Scenario set

The iOS suite contains Search and Book E2E, entry-rule, refinement, result-card, cabin, currency and
Flight Summary scenario families. App launch, onboarding, guest entry,
privacy handling and feature-flag setup are shared harness setup. A separate connectivity
test verifies that Appium launches the app and reaches a recognised landing entry point.

| Test | Journey | Passengers | Outbound | Inbound | Cabin |
|---|---|---|---|---|---|
| TC01 | One way LHR-JFK | 2 adults, 1 teen, 1 child, 1 infant | Virgin Atlantic | N/A | First available |
| TC02 | One way LHR-JFK | 2 adults, 1 teen, 1 child, 1 infant | Non-Virgin | N/A | First available |
| TC03 | Return LHR-JFK | 2 adults, 1 child | Virgin Atlantic | Virgin Atlantic | First available |
| TC04 | Return LHR-JFK | 2 adults, 1 child | Virgin Atlantic | Non-Virgin | First available |
| TC05 | Search entry validation | Passenger rules and limits | Calendar boundaries | N/A | N/A |
| TC06 | One way LHR-JFK | 1 adult | Focused sort, filter and refinement-interaction checks | N/A | N/A |

TC06 is split into independent classes for price, departure-time and duration sorting;
airline, connection and departure-window filters; sort/filter switching; persistence; and
Clear/reset behaviour. Airline tests use visible result-card carrier evidence first; when a card
does not show the selected carrier, they open Flight details and require at least one matching
operated leg. A short TC06 smoke retains the basic
sort-plus-filter Apply/Clear journey.

TC01-TC04 follow the same isolation boundary and each owns a concrete class and Appium session.
TC01 and TC03 are the representative one-way and return journeys through checkout and Trips. TC02
and TC04 stop at Flight Summary after proving partner and mixed-carrier behaviour; repeating the
same downstream checkout would increase runtime without covering another app contract.

BrowserStack can execute these classes concurrently. A local-session gate serializes them when
`RunMode=Local`, because one simulator and WebDriverAgent session cannot safely serve parallel
tests. Each test creates its own Appium session and search-results state.

The executable inventory is
`VAA.EAPI.Automation.MobileApp.UI/Contracts/search-book-automation-contract.json`. Keep it in
the same change as any scenario, category, platform, outcome, or stable-selector update. Offline
contract tests enforce its one-to-one mapping to the xUnit suite.

Carrier validation accepts selected-carrier evidence shown directly on a result card. When that
evidence is absent, it uses the operated-carrier text in Flight details because a card's primary
carrier does not describe every leg in a connecting itinerary.

### Flight Summary building blocks

TC24-TC26 select live inventory and validate the redesigned summary without continuing into
checkout. They cover Virgin-operated, other-operated and non-direct multi-carrier results. The
multi-carrier scenario discovers a result displaying `+1` rather than assuming a particular partner,
and requires its visible connection count and additional-carrier marker to survive selection.

These focused checks are intended to be composed into full E2E journeys as downstream screens become
complete. E2E flows must reuse the same summary assertion before passenger details, HPP, confirmation
and Trips; the focused tests remain valuable for fast failure localisation and should not be removed.

### Flexible dates, Edit Search and recovery gaps

TC27 selects a different app-owned flexible-date option, waits for the selection and results state,
and verifies the submitted route remains intact. TC28 uses a return search because the iOS app
intentionally exposes Edit Search only during inbound selection; it edits LHR-JFK to MAN-JFK and
checks the amended results route.

TC29 dynamically discovers the displayed fare and validates its included/not-included benefits,
then follows Flight Summary into Price summary and Fare conditions to validate the matching baggage
allowances. iOS PR #1200 originally added the IDs to a summary component that became unused after
the final Flight Summary redesign. The proposed iOS PR #1218 live-path change was reverted;
the contract remains pending. TC29 is explicitly skipped with `Readiness=OnHold` until the
live price-sheet and baggage contract is delivered and revalidated. TC30
is a skipped fail-once search-recovery contract pending a deterministic app test hook. Controlled
payment failure remains a documented backlog item.

## Acceptance-criteria coverage

### Search results and flight details

For every returned itinerary, validate the search response and native flight-details view
contain the same ordered segments. Each segment must expose:

- origin, destination, dates and times;
- connection and layover airport and duration where applicable;
- marketing flight number and marketing carrier;
- operating flight number and operating carrier;
- applicable itinerary or partner restrictions.

The native app currently provides indexed result, flight-details, segment and connection
identifiers. It does not expose separate stable fields for marketing carrier, operating
carrier, flight numbers or itinerary-limit text. Those identifiers are required before the
entire criterion can be asserted deterministically through Appium.

### Passenger, fare, partner restrictions and repricing

Validate restrictions for every passenger type and every segment before fare selection.
After selection/repricing, compare the selected itinerary with the basket:

- no segment is added, removed or reordered;
- marketing and operating carrier details remain unchanged;
- passenger counts and passenger types remain unchanged;
- segment prices add up to the displayed itinerary total;
- basket currency and total match the native summary and HPP amount.

This should combine native summary assertions with the captured Search and Book GraphQL
response and `retrieveBasket` response. GraphQL HTTP 200 responses must also be checked for
an `errors` collection.

### Persistence after payment

After confirmation, use the booking reference to validate the same ordered segment and
operating-carrier data in:

1. booking confirmation and HPP amount;
2. the created PNR/retrieve-booking response;
3. the confirmation-email event or rendered email payload;
4. the My Trips card and trip-details screen.

PNR and email verification are backend-correlated assertions and must reuse the EAPI
automation clients. They must not be inferred from the native screen. The mobile harness
therefore needs a post-booking verifier that accepts the booking reference, expected
itinerary and contact email, then polls the PNR and email sources within bounded timeouts.

## Remaining implementation work

- Add app-owned accessibility identifiers for marketing carrier, operating carrier,
  marketing/operating flight number and itinerary restrictions per segment.
- Capture a typed expected-itinerary model from the search GraphQL response.
- Compare native summary and basket repricing against that model.
- Revalidate the multi-passenger submit boundary after correcting the current passenger
  page's country-code input from `44` to `+44`.
- Add PNR, email and Trips correlation using the created booking reference.

## BrowserStack evidence

Validated against `bs://2b9f1ea73fdd63bfcc97f67643ce177963c06ed1`, aligned to
`vaa-mobile-ios` PR 1040 at `3f62f8ced7b9c14946c9818637ccaa09f5e33fc7`:

- TC01 passed: `8bd23dd0910c0d1551f0d32e933637b6b705b1ac`.
- TC02 passed: `1e1d478fbcb187da3971c690747a423af7717285`.
- TC03 passed: `2c9aa626b868b4261b90aa79a9d8e88774c585c5`.
- TC04 passed: `f239b8e05559ffbfbd8116588c08f21ddde9c70a`.

These green sessions cover search through flight summary. The scenarios were executed
sequentially in independent sessions to retain one-device usage while isolating application
state between journeys.

The four tests are now implemented through passenger/contact details, terms, HPP payment,
confirmation-reference capture and Trips verification. TC01 end-to-end pilots
`b30fe2364862219c6f79e9ade19cca1b7f053d87`,
`b70391050dbec410819c57c345d993ccf1394d3c` and
`ac5640466ddbb4a268f2f863deedac3b6d4e28fc` exposed two runtime findings:

- iOS correctly removes digits from passenger names; generated test names now contain
  letters only;
- after all five passenger forms, contact details and terms are completed, selecting the
  enabled Continue button leaves the app on Passenger details. BrowserStack showed that
  the current page rejected `44`; the shared flow now enters `+44` as an interim contract
  pending the passenger-page redesign. A 90-second diagnostic retry confirmed the failure
  was validation rather than latency, so the normal 30-second transition wait is retained.

The next diagnostic run should enable BrowserStack network response content and inspect the
GraphQL response body for passenger or basket validation errors at this boundary.

A subsequent diagnostic added a temporary two-second pause before selecting Continue to
secure payment. BrowserStack session `46070d60b63c05395aeba76fc978df9fb16964b8`
successfully created the payment reference and opened HPP, unlike the immediate-click run.
The HPP fields were entered, but Pay and confirm booking remained disabled beyond the
10-second state-based wait. The next investigation point is therefore HPP field validity;
the pause remains explicitly temporary and is not a general harness synchronisation rule.

Session `2c9139a1a47e0620db2f612121611c8deafb52dd` then verified that Address line 1,
Town/City and Postcode retained their expected values. HPP enabled Pay and confirm booking
and the test selected it. The next failure occurred after payment: the native confirmation
accessibility ID did not appear within the existing three-minute post-payment wait. Review
the session replay and payment response to determine whether HPP remained processing,
reported an error, or returned to a confirmation view with a changed automation contract.

Response-content diagnostic session `ce550caec00c74ec6b8e7bfd11c26e15b11bd08e`
completed HPP, displayed native confirmation and found the booking in Trips. For basket
`ad047a1f-b8d5-4edb-b967-9e271295c826`, `SearchAndBookCreatePaymentReference`,
`SearchAndBookUpdatePaymentStatus` and `SearchAndBookCreateOrder` each returned GraphQL data
without an `errors` collection. The test failed only because its confirmation assertion
expected a total passenger digit; the app exposes the passenger-type breakdown instead.
The assertion now validates each expected passenger type and count.

The final green-validation attempt intermittently returned to Try secure payment again.
Response capture in session `4539068ad007f447c7a67c769f4a8a8f730ff0c1` proved this was
not passenger validation or automation timing. Basket
`ca616376-f1b5-4ad7-b7de-8c699f2c63a6` was `VALID`: all five people, contact details and
terms were individually `VALID`, the payment amount was GBP 2,971.95 and seat price was
zero. `SearchAndBookCreatePaymentReference` returned HTTP 200 with GraphQL error
`Unhandled Exception in create PRI from payment mutation.` and
`orderPaymentReferenceId: null`. The app's retry-payment state is therefore the expected
UI error path for an intermittent payment-reference backend failure.
