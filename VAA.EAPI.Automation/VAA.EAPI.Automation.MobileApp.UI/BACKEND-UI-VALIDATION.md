# Backend-first UI baggage validation

TC41 (direct Virgin) and TC42 (connecting partner) run two parts in the same test execution:

1. Prepare a backend one-adult, one-way LHR–JFK revenue search for the same date used by the UI.
   Choose a returned Economy offer of the required itinerary type and retrieve its priced summary.
   TC42 requires exactly two ordered legs with at least one non-Virgin operator.
2. Keep the expected itinerary, exact fare title, GBP currency and baggage in memory.
3. Run the normal guest UI search. Match route, full dates, departure/arrival times, operating
   carrier and flight number for every leg, plus the transfer airport, from ordinary Flight details IDs.
4. Find the exact Economy fare title in the displayed cabin groups (partner names are not
   assumed to be Virgin's broad cabin names), verify GBP and Summary flight/fare retention, then
   compare baggage category, count, weight/units and dimensions. At least one dimension
   comparison must be exercised.

This is matched-offer parity across two searches, not shared-basket validation. No test-only
app metadata, Debug context hook, or app network interception is required. TC29 remains the
existing fast UI-to-UI test. The iOS changes required here are ordinary per-leg accessibility IDs.

## Runner setup

In the project's gitignored `local.settings.local.json`, set `BackendValidation:Enabled=true`.
The example and local file contain Environment, BaseUrl and TimeoutSeconds. The environment
must match `IOSLaunch:BuildProfile`. The shipped scope is SIT/DEV/UAT, never production.
The old BasketSubscriptionKey setting is removed: these Search BFF calls use the existing
guest backend automation endpoint contract, not the private Basket microservice.

The .NET runner makes both API calls, including when the app runs on BrowserStack. It needs
approved network access/VPN, DNS and TLS trust for the EAPI testresources origin. BrowserStack
device connectivity does not establish runner connectivity. No new CI runner/network setup
is included. Select the same test environment/build profile for app and API; without app
metadata this is configuration validation, not runtime proof of the installed app environment.

Both tests are included in `Suite=Regression`. The additional
`Suite=BackendRegression&RegressionArea=SearchAndBook` trait remains a diagnostic subset.
Local Regression runs require backend validation enabled; the workflow enables it automatically.
To run just the backend-comparison tests, use the following from the Automation solution:

```sh
dotnet test VAA.EAPI.Automation.MobileApp.UI/VAA.EAPI.Automation.MobileApp.UI.csproj -c Release --filter 'Category=MobileApp-SearchBook-BackendValidation' --results-directory TestResults/backend-first-regression --logger 'trx;LogFileName=regression.trx'
```

Analyse that exact TRX with the repository analyser.

## Reuse and safeguards

- Uses the same Search BFF request shape/routes as backend automation's OfferSearchBFFRequest,
  WebBFFEndpoint and WebBFFHelper.BuildSummaryUrl. No backend test assembly dependency.
- Reuses the existing UI search, accessibility IDs, fare-title parser, scoped flight-details
  reader, currency parser, baggage assertions, failure reporting and isolated session lifecycle.
- The narrow request wrapper retains validated TLS, no redirects, suppressed bodies, a 2 MiB
  response limit and per-request correlation. Core WebRequest is not used because its global
  timeout, permissive TLS and redirect behaviour do not meet this wrapper's requirements.
- Two preparation calls, no API calls per card/row, no retries or polling. Each call defaults to
  30 seconds, configurable 1–60. Successful overhead must be measured in live qualification.
- Summary pricing affects only the backend-owned search basket. It does not reprice the app
  basket. No payment, confirmation or durable booking is created.
- UI matching is bounded to 20 results; no-match is explicit and never substitutes another
  itinerary. Matching failure is not automatically an application bug.
- Backend data remains local to the test, not a shared/static cache, supporting remote parallelism.
  Raw responses and keys are not logged.

## Scope and status

TC41/TC42 are `Suite=Regression` and `Suite=BackendRegression`, `RegressionArea=SearchAndBook`, `Readiness=Ready`,
promoted after local passes on 30 September 2026. BrowserStack qualification remains separately
pending. Backend validation must be enabled for local Regression or diagnostic subset runs.
The scope is one-adult one-way
LHR-JFK Economy/GBP: direct Virgin (TC41) and a two-leg itinerary with a partner operator (TC42).
The baggage reference must match the backend Summary itinerary; it is not duplicated per leg.
Returns, other currencies, more than one connection and Android remain outside this slice.
These tests validate UI mapping, not upstream airline policy.

TC41 passed locally twice on 30 September 2026: the first scenario took 70.883s, with two allowance
rows and two dimension comparisons. Evidence: `TestResults/tc41-local-20260930-1452/local.trx`
and `TestResults/tc41-repeat-local-20260930/local.trx`. Both exact-TRX analyses reported no failures.
TC42 is implemented with offline coverage for ordered legs, wrong second-leg flight number,
wrong transfer airport, missing carrier and broken route continuity. It passed locally on
30 September 2026 in 153.037s with two allowance rows and two dimension comparisons.
Evidence: `TestResults/tc42-local-20260930/local.trx`; exact-TRX analysis found no failures.
BrowserStack qualification remains pending for both. No additional iOS changes are needed.

The previous basket-context approach has been removed, along with its app modifier, context
reader, credential setting and offline tests. Existing iOS accessibility work is preserved.
Earlier offline results alone did not establish a live pass; the TC41 runs above supersede that
initial status. Neither local scenario establishes BrowserStack qualification.
The BrowserStack recent-apps metadata request returned HTTP 200 with `No results found`;
a current uploaded iOS app containing the per-leg IDs must be confirmed before remote execution.

Final TC42 verification, 30 September 2026: **329 mobile offline/retry checks passed**;
exact-TRX analysis found no failures, project-scoped harness verification passed, and
`git diff --check` passed. Evidence: `TestResults/tc42-offline-20260930-final/offline.trx`.

Historical promotion verification (superseded by the opt-in split on 8 October): discovery listed 43 Search and Book scenarios,
including TC41 and TC42. All 329 offline/retry checks passed again with no failed tests in
the exact-TRX analysis (`TestResults/backend-regression-enrolment-20260930/offline.trx`).
No live rerun was performed during promotion; the app PR merges/rebuild are still awaited.

Initial TC41 verification, 30 September 2026: Release build and **325 mobile offline/retry checks passed**;
exact-TRX analysis reported no failures, and project-scoped harness verification passed.
Evidence: `TestResults/backend-first-offline-20260930-final/offline.trx` under the Automation
solution. iOS documentation validation passed with zero errors/warnings. The Summary view has
no remaining diff and the context modifier file is absent; unrelated app work is preserved.
The initial implementation verification did not run live APIs or the simulator; subsequent TC41
local evidence is recorded above. The source rollback does not uninstall/rebuild the running app.
