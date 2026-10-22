# Search & Book UI Playwright POC

This project is the first Search & Book UI automation slice inside `VAA.EAPI.Automation`.
It intentionally uses C# + Playwright and stays separate from the existing Selenium/Appium `Core.UI` harness.

## Page Object architecture

The suite follows the mandatory UI layering defined in `docs/FRAMEWORK_ARCHITECTURE.md`:

```text
Tests → journey orchestration → Pages → BasePage → Playwright session
                            ↘ Assertions
```

- `Pages/` owns Playwright APIs, selectors, waits, interactions and observable page state.
- `Assertions/` owns validation over captured page models or multiple stages of a journey.
- `Flows/` owns multi-page orchestration; it must not contain selectors or DOM scripts.
- `Support/` owns scenario data and non-browser helpers.
- `Tests/` describes scenarios and outcomes and must not call Playwright locator/driver APIs directly.
- `Tests/Setup/` is the narrow exception because it verifies that Playwright itself is installed and interactive.

All browser-backed page objects inherit `BasePage`. New screens or reusable hosted widgets must be
added as page/component objects rather than embedded in tests or journey orchestration.

## UI automation harness best practices

New frontend coverage must preserve the same boundaries and execution contract as the rest of the
EAPI Automation solution. Use this checklist when adding or reviewing a journey:

- **Express business intent in tests.** A test selects its scenario data, invokes a named journey
  flow and asserts the business outcome. Tests and feature steps must not construct browser
  sessions, access `Page`, define selectors or call locator/evaluation APIs.
- **Keep orchestration in flows.** Flows own session lifetime, navigation and multi-page sequencing.
  They call page objects and assertion helpers; they do not embed selectors or DOM scripts.
- **Keep browser interaction in pages.** Pages and component objects own stable accessibility,
  `data-testid` or `data-cy` selectors, condition-based waits, interactions and observable page
  state. Prefer stable product contracts over CSS structure or broad page-text matching.
- **Wait for evidence, never elapsed time.** Wait for the element, URL, response or retained value
  required by the next action. Do not add `Thread.Sleep`, arbitrary delays or blind retries. A
  narrowly bounded retry is acceptable only for a proven transient render and must verify the
  requested post-condition.
- **Use controlled test data and configuration.** Retrieve booking and loyalty data through the
  shared storage-table helpers. Keep secrets and populated settings in ignored local configuration
  or CI secret stores. Extend the existing environment/runner contract rather than introducing a
  feature-specific configuration path.
- **Make execution discoverable.** Apply the established `Category`, `tag` and work-item traits so
  the intended local, BrowserStack and scheduled packs select the test. If a scenario is deliberately
  excluded from regression, give it an explicit category and document the release condition.
- **Protect stateful journeys.** Run booking and payment scenarios only against approved
  non-production environments. Make booking creation explicit, do not automatically retry after
  order creation, and avoid parallel executions that can duplicate the same stateful scenario.
- **Capture safe diagnostics.** Produce a run-scoped TRX and analyse that exact run. Retain useful
  screenshots, traces and targeted network failures, but never log credentials, tokens, passenger
  details, loyalty data, payment data or unrestricted signed-in page/response bodies.
- **Keep local and remote behaviour equivalent.** The same page objects, flows, assertions and test
  filters must run locally and through BrowserStack. Remote-only setup belongs in shared session
  infrastructure, not in scenario tests.

Before committing, run the repository harness and the smallest relevant test category. Functional
browser runs use TRX output without code coverage and must be analysed against their run-specific
result file:

```pwsh
# From VAA.EAPI.Automation
scripts/verify.ps1 `
  -SkipBuild

dotnet test VAA.EAPI.Automation.SearchAndBook.UI/VAA.EAPI.Automation.SearchAndBook.UI.csproj `
  -c Release `
  --filter "Category=<target-category>" `
  --results-directory TestResults/<run-name> `
  --logger "trx;LogFileName=<run-name>.trx"

../scripts/analyse-test-run-results.ps1 `
  -TrxPath TestResults/<run-name>/<run-name>.trx `
  -SuiteName "Search & Book UI" `
  -EnvironmentName <environment>
```

## First-time setup

In the Codex app or Codex extension for VS Code, open this project folder and prompt:

```text
$setup
```

The repository-local skill checks prerequisites, restores and builds the project, installs Chromium, and runs an offline Playwright verification test with TRX analysis. It does not require credentials, VPN access, BrowserStack, or a live environment.

To run the same setup directly on macOS:

```bash
./Scripts/setup.sh
```

On Windows:

```pwsh
pwsh -NoProfile -File .\Scripts\Setup-SearchAndBookUi.ps1
```

Successful setup writes its TRX to `TestResults/search-and-book-ui-setup/` and its screenshot and trace to `TestResults/playwright-setup/`.

## Local smoke execution

Use the project runner for local regression execution. It builds the project, runs the selected regression tests, writes TRX output, and invokes the EAPI post-test analyser against that exact TRX:

```pwsh
.\VAA.EAPI.Automation.SearchAndBook.UI\Scripts\Run-SearchAndBookUiRegression.ps1 -EnvironmentName stg -BaseUrl "https://stg.virginatlantic.com"
```

Use `-InstallBrowsers` the first time on a machine, or when the Playwright package version changes:

```pwsh
.\VAA.EAPI.Automation.SearchAndBook.UI\Scripts\Run-SearchAndBookUiRegression.ps1 -EnvironmentName stg -BaseUrl "https://stg.virginatlantic.com" -InstallBrowsers
```

If `SAB_BASE_URL` is not set, the browser tests are skipped. This prevents normal offline gates from accidentally hitting a live website.

## Useful environment variables

- `SAB_BASE_URL`: Required target host, for example `https://stg.virginatlantic.com`.
- `SAB_BROWSER_CHANNEL`: Optional installed browser channel, for example `chrome` on a
  Heimdall Windows agent. When omitted, Playwright uses its bundled browser revision.
- `SAB_BROWSER`: `chromium`, `firefox`, or `webkit`; defaults to `chromium`.
- `SAB_HEADLESS`: `true` or `false`; defaults to `true`.
- `SAB_TRACE`: `true` or `false`; defaults to `true`.
- `SAB_ARTIFACT_DIR`: Directory for screenshots/traces; defaults to `TestResults/playwright`.
- `BROWSERSTACK_PREFLIGHT`: `true` captures the remote browser's egress IP and verifies the
  configured frontend before the scenario starts. The scheduled and manual BrowserStack workflows
  enable it and upload `browserstack-preflight.json` with the other session artifacts.
- `SAB_ORIGIN`: Deep-linked search origin; defaults to `LHR`.
- `SAB_DESTINATION`: Deep-linked search destination; defaults to `JFK`.
- `SAB_DEPARTURE_MONTH_OFFSET`: Departure month offset from today; defaults to `6`.
- `SAB_VOUCHER_DEPARTURE_DATE`: Optional `yyyy-MM-dd` travel date for backend-driven voucher
  frontend journeys. When omitted, the deterministic fallback is the 14th day six months ahead.
  Set this to a known reward-flight date when validating route-dependent Upgrade coverage.
- `SAB_ADULTS`, `SAB_TEENS`, `SAB_CHILDREN`, `SAB_INFANTS`: Passenger counts; default to `1,0,0,0`.
- `SAB_AWARD_SEARCH`: `true` for reward search deep links; defaults to `false`.
- Card-payment journeys submit the established non-3DS test payment and validate confirmation/PNR. The only exception is an authenticated STG journey, which intentionally stops once the HPP card form is usable because confirmation is unsupported there. Partial Pay with Points also submits its residual card payment outside that boundary; cardholder-offer scenarios retain their purpose-specific endpoint.
- `SAB_PASSENGER_FIRST_NAME`, `SAB_PASSENGER_LAST_NAME`: optional single-adult guest name overrides for guest journeys; defaults remain `Eapi Automation`.

## Environment mapping

- `https://stg.virginatlantic.com` is the STG frontend connected to the UAT backend.
- `https://vaa-test1.virginatlantic.com` through `https://vaa-test5.virginatlantic.com` are lower-environment frontends connected to the SIT backend.

Voucher redemption journeys are currently restricted in code to `vaa-test4` and are excluded from the STG BrowserStack matrix until the frontend feature is deployed elsewhere. The partner-itinerary voucher scenario is retained as skipped with an `Awaiting frontend delivery` reason.

The `Search & Book UI BrowserStack regression` workflow runs the full `Category=SearchAndBook-UI-Regression` pack on its schedule, including logged-in journeys and excluding `SearchAndBook-UI-Validation-NoSmoke`. Manual runs support `regression`, `vouchers`, and `custom` selections against STG or Test1-Test5; the legacy controls are no longer exposed. Both scheduled and manual runs build the test assembly once, discover the selected tests, and randomly assign each test method to exactly one browser. The GitHub run number is used as the seed, making an individual run's assignment reproducible without executing the same functionality on every browser. The parallel browser jobs use the same compiled artifact and the framework's existing BrowserStack and Key Vault secret mappings. Manual and scheduled matrices publish consolidated Slack results. Voucher selection is accepted only for Test4. Custom selection accepts any VSTest filter, such as `FullyQualifiedName~VoucherAndPointsBalanceTooLow`. Manual runs can distribute tests across every browser profile, run every selected test on every profile with `all-each`, or target only `mac-safari`, `windows-chrome`, or `windows-edge`.

Use UAT-partitioned test data and loyalty accounts for STG. Use SIT-partitioned data for Test1-5.
The frontend hostname and backend data partition are intentionally not assumed to have the same name.

## BrowserStack Playwright execution

For local execution, copy `local.settings.example.json` to `local.settings.local.json` in this
project and populate the values. The local file is ignored by Git and loaded automatically when
the test assembly starts. Existing process environment variables take precedence over values in
the file, while GitHub Actions continues to use repository secrets.

Set `SAB_RUN_MODE=BrowserStack` to use the shared Playwright BrowserStack connector. The connector
requires `BROWSERSTACK_USERNAME` and `BROWSERSTACK_ACCESS_KEY` and supports the existing project,
build, session, video, debug, network-log and Local-tunnel environment variables.

Each browser context first loads the configured base URL before entering a deep-linked search. This
matches the established WebdriverIO suite and allows STG edge and application session cookies to be
created before the first search GraphQL request. Consent cookies are scoped to
`.virginatlantic.com`, so that state remains valid across Virgin Atlantic subdomains.

```pwsh
$env:SAB_BASE_URL = "https://stg.virginatlantic.com"
$env:SAB_RUN_MODE = "BrowserStack"
$env:SAB_BS_OS = "Windows"
$env:SAB_BS_OS_VERSION = "11"
$env:SAB_BS_BROWSER = "chrome"
$env:SAB_BS_BROWSER_VERSION = "latest"
dotnet test .\VAA.EAPI.Automation.SearchAndBook.UI\VAA.EAPI.Automation.SearchAndBook.UI.csproj --filter "Category=SearchAndBook-UI-Regression"
```

STG uses UAT backend services regardless of whether the browser runs locally or in BrowserStack.
Enable `BROWSERSTACK_LOCAL=true` and provide `BROWSERSTACK_LOCAL_IDENTIFIER` only when the target
cannot be reached directly from the BrowserStack cloud.

Each BrowserStack session publishes its final assertion outcome before the remote browser closes.
Successful journeys report `passed` with a scenario-specific reason. If execution exits through an
assertion, timeout or unhandled exception before successful completion, session disposal reports
`failed` so the BrowserStack dashboard does not leave the result unmarked.

### Scheduled parallel matrix

`.github/workflows/search-book-ui-browserstack-smoke.yml` runs daily at 05:17 Europe/London and
can also be started manually. It discovers the selected test methods, shuffles them using the
GitHub run number as a reproducible seed, and distributes them evenly across these profiles with
`fail-fast: false`:

- Windows 11 / latest Chrome
- Windows 11 / latest Edge
- macOS Tahoe / Playwright WebKit

Each selected test method runs on exactly one browser per workflow run; theory rows remain together
on the browser assigned to their method. Every method is emitted as an independent workflow shard,
with up to 10 BrowserStack sessions running concurrently. A failed test does not cancel the other shards.
Booking shards do not retry automatically, avoiding duplicate bookings when a failure occurs after
order creation.
Each job uploads uniquely named TRX, screenshots and network diagnostics for its assigned method.
Failed BrowserStack sessions also attempt a final `failure.png` before teardown; status publication
and remote socket cleanup are best-effort and cannot replace the underlying test result.

Confirmation readiness requires a valid six-character PNR, the expected one-way or return flight
count, no visible interstitial/loader and the same rendered details across three consecutive polls.
The wait remains available for 180 seconds, but a visible `Uh oh`, unable-to-book or payment-failed
message fails immediately with its rendered text. This prevents a partial confirmation render from
being treated as complete while preserving fast feedback for genuine order-creation failures.

The final workflow job downloads those contracts and publishes one compact Slack notification with
totals, pass rate, failure buckets, top failures and any confirmed PNR/surname/scenario records.
Failures before VSTest starts, such as missing credentials, are reported in the
`workflow-setup` bucket instead of disappearing from the totals.

Failed BrowserStack sessions report the scenario and the action in progress. When the page contains
a recognised application failure such as an `Uh oh`, booking, or payment error, its visible message
is reported as an application failure. Element, timeout, assertion, and transport failures instead
include the exception type and message, current URL, screenshot, trace, and network diagnostics.

The scheduled regression intentionally targets current desktop browsers only. Mobile profiles are
not part of this pack. The project uses `BrowserStack.Playwright`; after that package is upgraded,
rerun the local regression script with `-InstallBrowsers` to install its matching local browser revision.

Guest card journeys complete payment and validate confirmation/PNR on STG and Test1-Test5.
Authenticated journeys stop at HPP only on STG, where confirmation is not supported for those test
accounts. On Test1-Test5, authenticated full-card and residual-card journeys continue through
confirmation and validate the PNR. The runner enforces these boundaries from the target host rather
than relying on workflow configuration.

The scheduled workflow requires repository secrets `BROWSERSTACK_USERNAME` and
`BROWSERSTACK_ACCESS_KEY`. Logged-in shards additionally require `LoyaltyAccount`,
`KEY_VAULT_URL`, `KEY_VAULT_TENANT_ID`, `KEY_VAULT_CLIENT_ID` and
`KEY_VAULT_CLIENT_SECRET`. That Key Vault principal must have read access to
`AzureStoragePnrCreationsConnectionString`; otherwise loyalty preflight fails before a BrowserStack
session is created. Manual regression runs include logged-in journeys by default; use a custom
VSTest filter when a narrower batch is required.

To submit the established STG non-3DS test card and validate confirmation, run a guest journey explicitly:

```pwsh
$env:SAB_BASE_URL = "https://stg.virginatlantic.com"
dotnet test .\VAA.EAPI.Automation.SearchAndBook.UI\VAA.EAPI.Automation.SearchAndBook.UI.csproj --filter "FullyQualifiedName~OneWay_VirginFare_GuestCardJourney_CompletesBooking"
```

This creates a real booking in the target test environment. The test validates a six-character
PNR, surname, passenger count, payment method and booked flight/cabin details.

## Logged-in journeys

Logged-in Search & Book tests select an adult Flying Club member from the existing
`LoyaltyAccounts` Azure Table. They do not store member details, passwords, or tokens
in the repository.

- `LoyaltyAccount`: password for the controlled Flying Club test-account pool.
- `SAB_LOYALTY_TIER`: optional account tier; defaults to `Red`.
- `SAB_LOYALTY_MIN_POINTS`: optional minimum points balance; defaults to `1`.
- `SAB_LOYALTY_ENVIRONMENT`: optional Azure Table partition override. STG defaults to
  `UAT` so its B2C login never receives a `SIT` account.

Logged-in regression journeys get the selected member from Azure Table Storage,
open the supported reward-search entry point, and complete the real browser B2C login.

Do not use an Entra client-credential token for this. It represents an application,
not the Flying Club member, and cannot create an equivalent browser session.

## Regression coverage

The scenario-based Playwright regression coverage contains 34 cases:

- 21 guest, logged-in, reward, currency, insurance, code-share, seat and payment-option journeys.
- 2 summary journeys covering insufficient points and an Economy Light upgrade.
- 11 passenger validation scenarios covering DOB rules, submit state and loyalty entry.

Run the complete regression category directly:

```pwsh
$env:SAB_BASE_URL = "https://stg.virginatlantic.com"
dotnet test .\VAA.EAPI.Automation.SearchAndBook.UI\VAA.EAPI.Automation.SearchAndBook.UI.csproj --filter "Category=SearchAndBook-UI-Regression"
```

Logged-in scenarios also require `LoyaltyAccount` and automatically use the UAT
Azure Table partition when the target is STG:

```pwsh
$env:SAB_BASE_URL = "https://stg.virginatlantic.com"
$env:LoyaltyAccount = "<controlled-pool-password>"
dotnet test .\VAA.EAPI.Automation.SearchAndBook.UI\VAA.EAPI.Automation.SearchAndBook.UI.csproj --filter "Category=SearchAndBook-UI-LoggedIn"
```

To certify guest card journeys through confirmation, run a controlled subset sequentially:

```pwsh
$env:SAB_BASE_URL = "https://stg.virginatlantic.com"
dotnet test .\VAA.EAPI.Automation.SearchAndBook.UI\VAA.EAPI.Automation.SearchAndBook.UI.csproj --filter "FullyQualifiedName~OneWay_VirginFare_GuestCardJourney_CompletesBooking" -- xUnit.MaxParallelThreads=1
```

Guest card journeys create real test bookings. Use the workflow's one-test-per-browser allocation and
avoid manually duplicating the same guest scenario across browsers.

`SearchAndBook-UI-Validation-NoSmoke` contains the empty-passenger-form disabled-button
assertion. It is intentionally excluded from regression execution until that product behavior
is released. Backend-driven voucher journeys now cover eligible VS itineraries, expiry exclusion,
partner-airline exclusion and Upgrade preference when Upgrade and Companion tie. These journeys
retrieve purpose-specific accounts from the loyalty storage tables and use the same controlled
account credential as the backend voucher tests. `LoyaltyAccount` can override that default;
local execution also requires the existing Search Key Vault settings.

The frontend voucher story mapping, release-dependent gaps and Confirmation payment journey are
documented in [VOUCHER_FE_COVERAGE.md](VOUCHER_FE_COVERAGE.md). The Confirmation journey completes
a permitted booking in the Test4 environment, consistent with the other booking journeys.

See `MIGRATION_COVERAGE.md` for the legacy mapping, STG boundary and remaining deliberate exclusions.

## Cookie consent

The Playwright harness primes Virgin Atlantic Ensighten consent cookies and local storage before navigation.
Each smoke test also attempts a best-effort visible banner dismiss after navigation. This prevents the cookie banner from blocking page assertions while still allowing a visible-banner fallback if consent storage changes.

## Passenger form and basket updates

The passenger page is React-controlled and paints usable inputs before all handlers and basket
state are settled. The shared `PassengerDetailsPage` implementation therefore:

- waits five seconds after the complete form is visible before entering data;
- paces input and select changes by 750 ms;
- enters each passenger and contact section once, then performs read-only retention checks;
- repairs the contact section once only when the post-consent retention check proves React cleared all contact values;
- selects insurance and consent once rather than rewriting passenger data after each render;
- registers checkout response listeners before clicking Continue;
- waits for successful `PUT /book/terms` and `PUT /book/passenger/{id}` responses before
  accepting navigation to seats or payment.

The application can still autosave passenger data while fields blur and send the final passenger
and terms state during submit. Those application-owned requests are expected. Tests must not add
extra repair writes unless a visible retained-value assertion proves that data was actually lost.
On failure, inspect `TestResults/playwright/trace.zip` for the basket ID, request payloads, response
status and response body. A visible `Uh oh` modal after successful passenger updates can be caused
by the subsequent `POST /book/payment` payment-initialisation request.

The HPP billing-country autocomplete is also encapsulated in `PaymentPage`. Its Angular Material
overlay can be attached and active outside the iframe's host viewport, so selection is dispatched
inside the HPP DOM and verified from the retained input value rather than relying on screen geometry.

## Outputs

The runner writes:

- TRX: `TestResults/search-and-book-ui-<env>-regression/search-and-book-ui-<env>-regression.trx`
- Playwright screenshot and trace: `TestResults/playwright-<env>-regression/`
- Post-test analysis: console output from `..\scripts\analyse-test-run-results.ps1`

## Migration intent

The WDIO/Cucumber project should not be copied across directly. Port the valuable coverage and diagnostics into this harness incrementally:

1. Keep the regression category aligned with stable end-to-end business journeys.
2. Reuse EAPI backend/API helpers for data setup and diagnostics.
3. Add released voucher and payment behaviours to the appropriate environment-specific packs.
4. Retire duplicated repo-specific helpers once equivalent shared harness support exists.
