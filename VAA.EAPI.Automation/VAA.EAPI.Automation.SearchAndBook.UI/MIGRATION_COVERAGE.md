# Search and Book Playwright migration coverage

This document tracks migration from `search-and-book-e2e` into the EAPI C# Playwright harness.
The legacy feature files remain the behavioural reference until each area is implemented and
verified here.

## Implemented

The active suite is organised around business journeys rather than progressive TC01-TC12 checks.
`GuestJourneyTests` contains 21 data-driven journey cases:

- One-way Economy Light and Upper Class guest journeys through confirmation.
- One-way Premium journey with a paid seat.
- One-way Upper Class journey through available-seat selection.
- Return Economy Light and Upper Class guest journeys through confirmation.
- Return Premium journey with paid outbound and available inbound seats.
- Return Upper Class journey with per-leg available-seat selection.
- One-way and return Economy Light for two adults, one child and one infant.
- USD, GBP, EUR and CAD Point-of-Sale routes.
- USD and GBP insurance-acceptance variants.
- One-way and return Delta-operated journeys.
- Logged-in one-way Economy Light for two adults, one child and one infant.
- Logged-in return Economy Classic reward journey for two adults, one child and one infant.
- US cardholder-offer journey that validates the offer and opens the Synchrony application.
- Logged-in return Premium journey for two adults using partial Pay with Points and card residual.

These cases use random departure dates three to seven months ahead and return dates seven to
twenty-eight days later. Guest card cases validate fare/carrier targeting, price continuity, extras,
passenger composition, seat selection, payment and confirmation/PNR. Authenticated cases stop at an
interactable HPP card form.

`SummaryJourneyTests` covers the low-balance reward warning and disabled Continue control,
plus the Economy Light summary upgrade through to an interactable HPP card form. The upgrade
asserts the success state, target cabin and increased basket total.

`PassengerValidationTests` migrates all 11 legacy passenger-validation scenarios: adult, teen,
child and infant DOB boundaries; valid and empty submit states; Virgin loyalty-number entry;
month range, leap-year, non-leap-year and 30-day-month filtering. The empty-form scenario retains
its legacy non-smoke classification because that behavior is not yet released.

## STG boundary

The scheduled STG matrix now completes ordinary guest card journeys through payment and
confirmation. Logged-in STG journeys stop once HPP is usable; this boundary is represented in the
generated matrix and enforced again by the runner. Purpose-specific validation, cardholder-offer and
summary scenarios continue to stop at their asserted endpoint.

The 18 August 2026 STG investigation established the passenger-page basket sequence. React can
render the form before its state handlers settle, while the application also autosaves passenger
changes on field blur and sends final passenger and terms state during submit. The Playwright page
object now uses shared paced-entry helpers, read-only retention assertions and explicit waits for
the terms and passenger checkout responses. It does not rewrite valid data after consent changes;
one guarded contact repair is allowed only when a read-only post-consent check proves React cleared
the complete contact section.

One diagnostic run returned a valid basket from both passenger and terms updates but received HTTP
500 from `POST /flight-only/api/checkout/uat/book/payment`. After removing redundant automation
writes, the same Economy Light journey returned HTTP 200 from payment initialisation and reached
the HPP boundary successfully. The trace remains the source of truth for basket IDs and API bodies.

TC10 was then executed through payment and confirmation on STG on 18 August 2026. Payment
initialisation, payment-status update and confirmation all returned HTTP 200; the test created a
six-character PNR and passed its surname, passenger-count, Visa payment and flight-summary checks.
This proves the confirmation implementation used by the scheduled guest booking shards.

Guest card journeys now always complete through confirmation. On 18 August 2026, representative return paid-seat,
one-way Upper Class, multi-passenger and GBP-with-insurance journeys all progressed through their
implemented browser interactions. STG then returned genuine checkout failures at different
downstream boundaries: payment initialisation HTTP 500, post-payment retrieval HTTP 500, or
confirmation HTTP 422. The diagnostics now record the basket ID and failed response body in
`network-diagnostics.json`. These remain failed tests and are not converted into accepted passes.

The US cardholder-offer journey reached payment, validated the offer and clicked Apply now. STG
displayed its credit-card-application outage banner, which the automation reported verbatim.

Test2 was compared against the same one-way Economy Light journey on 18 August 2026. After the
runner was corrected to wait for the passenger form before reading its price summary, the journey
passed to an interactable HPP. A full-payment run then returned HTTP 200 confirmation with PNR
`CIBQUC`, surname `AUTOMATIONONE`, one adult, Visa payment, VS3 LHR-JFK, Economy Light and a
GBP 806.59 total. The migrated guest-surname expectation was corrected to match its generated
test data. A subsequent verification attempt reached confirmation but returned HTTP 422 code
`7522`: PaymentEngine information could not be retrieved for basket
`7793f4d6-5fbb-4b26-b23d-57be7dc5b853`, error ID
`61c7c8ba-4623-4237-b4bd-4cafcf9f49ed`. This is retained as an environment failure.

BrowserStack Playwright connectivity was verified directly against the STG frontend/UAT backend
on Windows 11 with latest Chrome, without a BrowserStack Local tunnel. The homepage test passed in
15 seconds and the deep-linked one-way search returned flight results in 20 seconds. The migrated
Economy Light journey also progressed through remote search, summary and passenger submission.
Two cloud-specific harness assumptions were corrected: URL matching now waits only for navigation
commit before applying the real flight-card readiness predicate, and checkout response waits ignore
intermediate HTTP 3xx redirects while still accepting terminal success or failure responses. The
final remote journey then reached the existing UAT payment-initialisation failure: HTTP 500 from
`POST /en-EU/flight-only/api/checkout/uat/book/payment`, error ID
`3e7cb81f-e2f5-47f3-be77-b8438c07d993`.

BrowserStack result publication is now part of the shared session lifecycle. A Windows 11/latest
Chrome verification run reported `passed` through the BrowserStack API with the reason
`Virgin Atlantic host loaded successfully.` Early assertion, timeout and exception exits fall back
to a `failed` result during session disposal rather than leaving the remote session unmarked.

The scheduled regression uses three desktop profiles: Windows 11/latest Chrome, Windows 11/latest
Edge and macOS Tahoe/Playwright WebKit. It discovers `Category=SearchAndBook-UI-Regression`, shuffles
test methods using the GitHub run number as a reproducible seed and assigns every method to exactly
one browser. Every method runs as its own workflow shard, with up to 10 BrowserStack sessions in
parallel and unique result artifacts; theory rows remain together within their assigned shard.
Mobile profiles and the legacy smoke category are not part of the scheduled pack.

The return Upper Class per-leg seat journey also passed remotely after exercising the passenger-page
seat-choice popup variant. The migrated handler recognises both modal copy variants, selects the
requested CTA and prefers an included seat for non-paid-seat journeys so checkout total continuity
remains meaningful. BrowserStack transport shutdown and status-publication errors are isolated from
the journey result, and failed sessions attempt a final screenshot before teardown.

The scheduled regression contains 23 test methods, expanding to 31 xUnit cases after the unreleased
empty-passenger-form assertion is excluded. Card-payment cases continue to confirmation and log
`[confirmed-booking]` PNR evidence, including authenticated full-card and residual-card journeys on
Test1-Test5. Only logged-in STG journeys stop at HPP. Each browser shard has final-TRX analysis and per-shard artifacts. UAT loyalty
tests consume the established EAPI Key Vault configuration and a controlled Flying Club password
secret. Aggregated reporting reads JSON result contracts rather than scraping console text, so
test-start and credential failures are included in totals.

Focused real-device verification on 19 August 2026 connected successfully to the latest stable
matrix profiles. Android 17 completed the passenger DOB validation on its first attempt. Further
iOS and Android booking journeys exposed repeatable STG `/flights/search/api/graphql` HTTP 401
responses: iOS retained flight cards with no enabled fares, while Android remained on an incomplete
summary with a £0 total. The harness now retries that incomplete summary once by reloading, then
retains the failure and response body in network diagnostics if STG remains unauthorized. This is
kept as an environment/application failure rather than bypassing disabled controls.

## Remaining migration

- PayPal and other payment variants not present in the legacy feature pack.
- Full migrated-matrix certification once STG order creation is stable and controlled UAT loyalty
credentials are available to the execution process.

On 19 August 2026, the Windows 11/latest Edge Premium paid-seat BrowserStack shard selected a paid
seat, submitted HPP payment and called the confirmation endpoint. STG returned HTTP 500 `G009`
with error ID `91131530-5bb6-45c2-bf68-0189fc161a2f`, so the test correctly failed rather than
claiming a booking. The JSON result contract now buckets this as `confirmation-processing` and
includes the checkout response evidence for Slack. Authenticated verification was stopped during
test-data preflight because the available local Key Vault principal could not read
`AzureStoragePnrCreationsConnectionString`; no BrowserStack session or payment was created. The CI
loyalty principal must be granted that secret before those shards can execute.

The isolated Windows 11/latest Chrome Economy Light row then completed successfully in 3 minutes
34 seconds, created PNR `CI42DC` for surname `AUTOMATIONONE`, and passed confirmation passenger,
payment and flight assertions. Its TRX result contract captured the PNR, surname and scenario for
the aggregated Slack notification.
- Voucher coverage uses live backend redemption data for VS eligibility, outbound-expiry
  exclusion, partner-airline exclusion and Upgrade-over-Companion tie preference.
- Desktop Safari engine coverage is represented by Playwright WebKit because Playwright does not
  automate branded desktop Safari directly.
- Legacy inspection-only scenarios remain intentionally retired rather than becoming scheduled
  assertions.
