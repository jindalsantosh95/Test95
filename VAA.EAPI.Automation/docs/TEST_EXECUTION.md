# Test Execution Guide (VAA.EAPI.Automation)

This document provides practical commands to build and run automation tests within `VAA.EAPI.Automation.sln`.

For mobile Search and Book release sign-off, use the
[Sanity/Regression release-pack guide](../VAA.EAPI.Automation.MobileApp.UI/RELEASE-PACKS.md).
It defines 14 Sanity and 54 Regression scenarios including two backend comparisons and card-presentation TC55. Always scope by `RegressionArea=SearchAndBook&Platform=iOS`.
Both packs include real test booking/payment journeys; use an approved test environment.
Pack enrollment is not release approval: retain run-specific evidence and review all exclusions.

For the opt-in mobile UI/backend baggage comparison, see
[backend-first validation](../VAA.EAPI.Automation.MobileApp.UI/BACKEND-UI-VALIDATION.md).
API requests execute on the .NET runner even when Appium targets BrowserStack. Provision runner
network access and ignored settings separately from device access. It uses normal flight-detail
accessibility IDs and no app metadata hook. TC41/TC42 are included in
`Suite=Regression`; `Suite=BackendRegression&RegressionArea=SearchAndBook` remains a diagnostic
subset filter. Enable `BackendValidation:Enabled=true` for local Regression runs; the workflow
enables it automatically for Regression.

## Run from Solution Folder

```bash
cd VAA.EAPI.Automation
```

## Baseline Commands

```bash
dotnet restore VAA.EAPI.Automation.sln
dotnet build VAA.EAPI.Automation.sln -c Release
```

## Run a Specific Test Project

```bash
dotnet test VAA.EAPI.Automation.MyAccount/VAA.EAPI.Automation.MyAccount.csproj -c Release
```

Swap `MyAccount` for the target suite (e.g., `Trips`, `CRM`, `MarketingLoyalty.UI`).

## Run by Trait / Category Filter

> Use existing trait names from the project under test.

```bash
dotnet test VAA.EAPI.Automation.Trips/VAA.EAPI.Automation.Trips.csproj -c Release --filter "Category=SelectFreeSeats-Regression"
```

### MyAccount IdentityMS-related GraphQL tests

```bash
dotnet test VAA.EAPI.Automation.MyAccount/VAA.EAPI.Automation.MyAccount.csproj -c Release --filter "Category=IdentityMSRelatedGraphqltests"
```

This category selects MyAccount GraphQL flows whose operation or resolver invokes or depends on IdentityMS. Shared enrolment or authentication used only to arrange test data does not qualify.

The MyAccount GraphQL resolver categories can also select verified downstream service paths:

```bash
dotnet test VAA.EAPI.Automation.MyAccount/VAA.EAPI.Automation.MyAccount.csproj -c Release --filter "Category=MemberManagementMSRelatedGraphqltests"
dotnet test VAA.EAPI.Automation.MyAccount/VAA.EAPI.Automation.MyAccount.csproj -c Release --filter "Category=VRedMSRelatedGraphqltests"
dotnet test VAA.EAPI.Automation.MyAccount/VAA.EAPI.Automation.MyAccount.csproj -c Release --filter "Category=CustomerMSRelatedGraphqltests"
dotnet test VAA.EAPI.Automation.MyAccount/VAA.EAPI.Automation.MyAccount.csproj -c Release --filter "Category=CustomerTransactionMSRelatedGraphqltests"
```

Book GraphQL E2E selectors:

```bash
# Curated daily end-to-end confidence pack.
dotnet test VAA.EAPI.Automation.Book/VAA.EAPI.Automation.Book.csproj -c Release --filter "Category=Book-Core-Regression"

# Broader Book end-to-end regression pack. This includes the scheduled core journeys plus existing E2E regression scenarios.
dotnet test VAA.EAPI.Automation.Book/VAA.EAPI.Automation.Book.csproj -c Release --filter "Category=Book-E2E-Regression"

# Full Book GraphQL regression umbrella, including endpoint-level Basket, Seat, Insurance, Pay With Points, WebBFF, and E2E tests.
dotnet test VAA.EAPI.Automation.Book/VAA.EAPI.Automation.Book.csproj -c Release --filter "Category=BookGraphQl-Regression"
```

UI/BDD examples:

```bash
# WiFi portal BDD tests (all scenarios)
dotnet test VAA.EAPI.Automation.MyAccount.UI/VAA.EAPI.Automation.MyAccount.UI.csproj -c Release --filter "Category=WifiPortalBDD"

# WiFi portal — specific feature
dotnet test VAA.EAPI.Automation.MyAccount.UI/VAA.EAPI.Automation.MyAccount.UI.csproj -c Release --filter "DisplayName~SignIn"

# WiFi portal — password reset validation only
dotnet test VAA.EAPI.Automation.MyAccount.UI/VAA.EAPI.Automation.MyAccount.UI.csproj -c Release --filter "DisplayName~PasswordResetValidation"
```

WiFi portal tests run on local headless Chrome by default. Set `BROWSERSTACK_USERNAME` and `BROWSERSTACK_ACCESS_KEY` in config to run on BrowserStack across multiple browsers/devices.

## Collect Test Results

```bash
dotnet test VAA.EAPI.Automation.sln -c Release --logger "trx;LogFileName=automation.trx"
```

For local targeted runs, prefer writing the TRX into a run-scoped results directory so post-test analysis can target the
current run rather than older files in `TestResults`:

~~~pwsh
dotnet test <project>/<project>.csproj -c Release --filter "<filter>" --results-directory TestResults\<run-name> --logger "trx;LogFileName=<run-name>.trx"
~~~

## Post-Test Analysis

After a local `dotnet test` run that produced TRX output, run the repo-root analyser
against the exact TRX file, or ask the user whether they want the post-test analysis
included.

Run from the repository root:

~~~pwsh
scripts\analyse-test-run-results.ps1 -TrxPath TestResults\<run-name>\<run-name>.trx -SuiteName "<suite>" -EnvironmentName <env>
~~~

Use `-TrxPath` for single local runs. Use `-ResultsDirectory` only when intentionally
analysing a whole results folder that may contain multiple TRX files.

## Suggested Local Validation Strategy

1. Restore/build solution once.
2. Run the directly impacted project.
3. Produce a named TRX for user-requested test runs.
4. Run, or offer to run, `scripts\analyse-test-run-results.ps1` against the exact TRX.
5. Run one adjacent suite if shared helpers/framework code changed.
6. Use trait/category filtering for faster iteration.

## Manifest-driven PNR data creation

Use `VAA.EAPI.Automation.Utility.DataProvisioning` when the purpose is to create synthetic Book PNRs for Trips or OCI consumers rather than assert a regression test. It accepts an external JSON manifest, uses payment APIs for frictionless cards by default, retains Playwright for PayPal or an explicit `cardPaymentMode: "Hpp"` override, can publish successful PNRs to explicitly configured Azure tables, and produces a structured result per scenario.

Validate a checked-in catalogue without creating data:

~~~pwsh
dotnet run --project .\VAA.EAPI.Automation\VAA.EAPI.Automation.Utility.DataProvisioning\VAA.EAPI.Automation.Utility.DataProvisioning.csproj -- .\VAA.EAPI.Automation\VAA.EAPI.Automation.Utility.DataProvisioning\defaults\pilots\trips.json --validate-only
~~~

See [Data provisioning Playwright pilot](DATA_PROVISIONING_PLAYWRIGHT_PILOT.md) for prerequisites, local Windows/macOS commands, catalogue contents, storage schemas, output semantics, safety rules and the scheduled-workflow rollout design. The existing Trips PNR-creation workflow still executes the legacy xUnit categories until that rollout is completed.

The Book GraphQL end-to-end suite also defaults ordinary frictionless card scenarios to API payment submission. PayPal, `ThreeDS`/`is3DS`, and co-brand/Synchrony scenarios retain their existing HPP implementation. Set `BookCardPaymentMode` to `Hpp` for a complete run that must exercise the HPP card UI; `Api` is the default. An invalid value fails before card submission rather than silently choosing a path.

## Notes

- Full solution test runs may be long and environment-sensitive.
- Prefer targeted runs for feature work, then expand scope for shared framework changes.
- OCI execution is capped at four parallel test cases. Selenium HPP use is capped separately at two browser sessions by default; set `HPP_SELENIUM_MAX_CONCURRENT_SESSIONS` to a value from 1 to 8 only after validating the runner capacity.
- Selenium sessions register their ChromeDriver process when it is created. Cleanup closes WebDriver normally and then terminates only survivors from that registered browser tree; shared runners must never use global Chrome cleanup.

## Native Mobile xUnit Execution

Run the focused functional validation (TC05 search-entry rules and TC06 result refinement):

~~~pwsh
dotnet test VAA.EAPI.Automation.MobileApp.UI/VAA.EAPI.Automation.MobileApp.UI.csproj -c Release --filter "Category=MobileApp-SearchBook-FunctionalValidation" --results-directory TestResults\mobile-search-book-functional-validation --logger "trx;LogFileName=mobile-search-book-functional-validation.trx"
~~~

TC06 uses independent test classes so BrowserStack can execute focused sort, filter and
interaction checks concurrently. Local runs are automatically serialized around Appium session
creation because a single simulator cannot safely accept concurrent WebDriverAgent sessions.

Run the full Search and Book smoke E2E journeys (TC01-TC04):

~~~pwsh
dotnet test VAA.EAPI.Automation.MobileApp.UI/VAA.EAPI.Automation.MobileApp.UI.csproj -c Release --filter "Category=MobileApp-SearchBook-SmokeE2E" --results-directory TestResults\mobile-search-book-smoke-e2e --logger "trx;LogFileName=mobile-search-book-smoke-e2e.trx"
~~~

TC01-TC04 are deliberately implemented as four concrete test classes. BrowserStack may execute
them concurrently with independent Appium sessions; local runs wait on the harness session gate
and execute one at a time. New functional scenarios must preserve this pattern: one complete,
independently runnable outcome per concrete class, shared mechanics in a base/flow/page object,
and no ordered dependencies between tests.

For a single BrowserStack run with failure-only network reporting, use:

~~~pwsh
scripts/run-mobile-search-book-diagnostics.ps1
~~~

The wrapper requires `BrowserStack.NetworkLogs` and `BrowserStack.CaptureNetworkContent` to be
explicitly enabled in the gitignored local settings file because BrowserStack cannot enable them
retrospectively. Committed defaults leave content capture off. The failure hook queries that
original session only when a test fails
and writes only an
allowlisted summary: method, host/path, HTTP status, GraphQL operation/error codes and correlation
identifier. Request/response bodies, query strings, variables and backend messages are never
written to xUnit output. The test and payment are not retried.

The mobile project uses direct xUnit tests and Appium page objects; Cucumber feature files
are not required. BrowserStack credentials and the `BrowserStack.App` ID belong in the
gitignored `local.settings.local.json` created from the committed safe example.

For PR/issue-driven test authoring, the `$mobile-qa-authoring` skill routes agents through the
machine-readable Search & Book contract, reuse checks, app-owned selector requirements, offline
gates, and explicit authorization boundaries for live booking/payment tests.

For initial setup or a safe connectivity check, use:

~~~pwsh
scripts/setup-mobile-app-ui.ps1
~~~

This runs only offline harness tests and `Category=MobileApp-Connectivity`. The connectivity
test requires a valid mobile configuration, creates an Appium session, and verifies a
recognised onboarding, primary-navigation, or native Search & Book entry point. Do not run the
mobile project without a filter: its Search and Book smoke category contains real booking
and payment journeys.

The current mobile tests target iOS. They can be launched against BrowserStack from Windows,
macOS or Linux; local iOS execution requires macOS, Xcode, Appium and XCUITest. Android
driver support exists in the shared framework, but Android test execution is not currently
validated by this suite.

## Historic Trips SCV configuration

Historic Trips automation uses the existing `Automation.Core.Config` / `ConfigReader`
pattern. Set these environment variables in the test runner before starting the
process. All three settings are required; there are no account, database or container defaults.

| Setting | Value to configure |
| --- | --- |
| `ScvAccountEndpoint` | Target SCV Cosmos account endpoint |
| `ScvDatabase` | Target SCV database name |
| `ScvCollection` | Target SCV container name |

The helper remains SIT-only. Cosmos authentication still resolves
`TESTING-OrderDB-Automation-Key` from the configured Key Vault; the selected endpoint
must be compatible with that credential. Missing settings fail before any Key Vault or Cosmos call. No account key belongs in these settings.

## Find Trip By Flight Date contract

The Trips FindTripByFlightDate regression requests use flightDate (ddMMMyy), the
scheduled local departure date of any SCV segment, instead of the booking creation
date. The existing SIT SCV data source selects a local departure and verifies
that PNR, departure and same-passenger names identify exactly one stored order.
Zero or multiple matches must return an empty list. The response bookingDate still
represents creation. Run the deployed regression only after Order Data/APIM and
Trips GraphQL both expose the revised input contract.
