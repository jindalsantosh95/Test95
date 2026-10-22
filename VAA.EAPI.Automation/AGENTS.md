# AGENTS.md — VAA.EAPI.Automation

## Scope
These instructions apply to all files under `VAA.EAPI.Automation/`.

## Purpose
Use this folder for API, GraphQL, SOAP, and UI/mobile automation projects contained in `VAA.EAPI.Automation.sln`.

## Principles (read first)
Before designing or implementing a change, read the relevant principle document in
[`docs/principles/`](docs/principles/README.md). They are the normative layer above the
conventions below — what to decide, and why:

- [Test Design](docs/principles/test-design-principles.md) — determinism, independence, traits, assertions
- [Architecture](docs/principles/architecture-principles.md) — layering and change placement (enforced by `ArchitectureTests`)
- [Test Data](docs/principles/test-data-principles.md) — storage-table-driven, synthetic, base URLs
- [Integration](docs/principles/integration-principles.md) — request wrappers, GraphQL vs REST assertions, JSON navigation
- [Execution & Infrastructure](docs/principles/execution-infrastructure-principles.md) — `verify.ps1` gate, ratchet, Heimdall runner
- [Security](docs/principles/security-principles.md) — secrets, PII, least privilege

When new code conflicts with older nearby code, follow the principle.

## Preferred workflow
1. Identify the target project and feature area first.
2. Reuse existing helpers, fixtures, page objects, and request wrappers before adding new primitives.
3. **For any test that needs PNR / loyalty / booking data, source it from the shared Azure storage tables via `VAA.EAPI.Automation.Core.Helpers.StorageAccount.RetrieveFromStorageTable` — do not hard-code.** See [`docs/TEST_DATA_STRATEGY.md`](docs/TEST_DATA_STRATEGY.md) for the canonical pattern and the narrow exceptions.
4. Keep changes project-local unless patterns are reused across multiple suites.
5. Update docs when adding new traits, test-data flows, or execution paths.

## Mobile QA setup requests

When a user asks to "setup", configure, onboard, or check connectivity for the native
Mobile App UI tests, use the repo-local `mobile-qa-setup` skill and run:

```pwsh
scripts/setup-mobile-app-ui.ps1
```

This is the safe newcomer path. It creates the gitignored configuration from the safe example,
validates the selected local or BrowserStack mode, performs offline checks and runs only
`Category=MobileApp-Connectivity` when configuration is complete. Never use an unfiltered
`dotnet test` as a setup check because that can execute real booking and payment journeys.

## In-Flight Applications UI run requests

When a user asks to run, set up, or verify the "In-flight automation", "In-flight applications",
"onboard WiFi", or Starlink/captive-portal tests, use
`VAA.EAPI.Automation.InFlightApplications.UI` and read its `README.md` before running anything.

For an unqualified request such as "run the In-flight automation", use the safe, non-stateful pack:

```pwsh
VAA.EAPI.Automation.InFlightApplications.UI/Scripts/Run-Regression.ps1
```

Before the first run, confirm that the gitignored project-local `local.settings.local.json` exists,
that `IN_FLIGHT_APPLICATIONS_UI_BASE_URL` identifies an approved non-production environment, and
run `Scripts/Setup.ps1` if Chromium has not been installed for this checkout. Never print or commit
the populated settings file.

Do not infer permission to run the stateful packs. Run `Scripts/Run-OtpJourneys.ps1` only when the
user explicitly requests OTP/adult-enrolment coverage, and `Scripts/Run-AccountJourneys.ps1` only
when they explicitly request shared-account login/password-reset coverage. These packs can create
Flying Club accounts or mutate shared accounts. Their scripts disable Playwright tracing to protect
credentials and OTPs. Run the three packs sequentially, never concurrently.

Each runner writes a uniquely named folder beneath the project's `TestResults` directory. Report
passed, failed, and skipped totals and link the generated TRX; do not expose credentials or OTPs in
the summary.

## New Playwright UI projects

For a new web UI automation project, use both the repo-local `api-ui-automation` and
`new-playwright-ui-project` skills. Start with the deterministic scaffolder instead of copying an
older suite:

```pwsh
pwsh -NoProfile -File skills/new-playwright-ui-project/scripts/new-playwright-ui-project.ps1 `
  -DomainName <PascalCaseDomain> `
  -Description "<business purpose>"
```

The generated page-object boundary is mandatory. Customise the starter page, flow, assertion, and
test; do not collapse browser calls back into tests or flows.

## New scheduled automation suites

When an automation project needs a new scheduled or manually dispatched GitHub Actions suite, use
the repo-local `new-automation-suite` skill and its deterministic scaffolder. Do not copy a large
existing workflow. The generated suite manifest is the durable source for its project, categories,
runner, timeout, reporting configuration, and required setting names; the thin workflow owns only
triggers, target-environment selection, and secret mapping.

## Mobile functional-test authoring

For native Appium tests, one independently runnable scenario belongs in one concrete xUnit test
class. A complete scenario remains an end-to-end outcome; never create ordered tests for search,
payment, confirmation, or Trips. Put shared mechanics in page objects, flows, or abstract test
bases, and retain the harness traits and `SearchBookTestBase` failure reporting.

This class boundary gives each scenario an isolated Appium session and allows BrowserStack to run
classes concurrently. Do not add these scenarios to the non-parallel `Mobile` collection.
`RunMode=Local` is serialized automatically by `MobileFixture`'s local-session gate because one
simulator cannot safely accept concurrent WebDriverAgent sessions. Use the repo-local
`mobile-qa-authoring` skill for PR, issue, or acceptance-criteria-driven mobile test changes.

## Command baseline (run from this folder)
```bash
dotnet restore VAA.EAPI.Automation.sln
dotnet build VAA.EAPI.Automation.sln -c Release
```

Targeted execution examples:
```bash
dotnet test VAA.EAPI.Automation.MyAccount/VAA.EAPI.Automation.MyAccount.csproj -c Release
dotnet test VAA.EAPI.Automation.Trips/VAA.EAPI.Automation.Trips.csproj -c Release --filter "Category=SelectFreeSeats-Regression"
```

When running local tests for a user, add a TRX logger by default and then run the repo-root
post-test analyser against the exact TRX file that was just generated. This avoids stale
TRX files from earlier local runs being included in the analysis. This applies to requests
such as "run unit tests", "run automation tests", "run the Book smoke tests", or "run this
regression filter":

```pwsh
dotnet test <project>/<project>.csproj -c Release --filter "<filter>" --results-directory TestResults\<run-name> --logger "trx;LogFileName=<run-name>.trx"
..\scripts\analyse-test-run-results.ps1 -TrxPath TestResults\<run-name>\<run-name>.trx -SuiteName "<suite>" -EnvironmentName <env>
```

If running unit tests outside `VAA.EAPI.Automation.sln`, add `--collect:"XPlat Code Coverage"`;
the analyser will append a coverage table when Cobertura output exists. Automation functional
tests stay TRX-only by default unless the user explicitly requests coverage.

If a user explicitly asks for a raw/quick test run, skip the analyser and say that no TRX
analysis was run.

## Harness verification
This solution uses harness engineering — see [`docs/HARNESS_ENGINEERING.md`](docs/HARNESS_ENGINEERING.md).
Before pushing, run the local sensor from this folder:

```pwsh
scripts/verify.ps1
```

It runs the analyzer ratchet (`dotnet build`), the offline test suites (architecture
fitness + `Core` behavior), and a formatting check on new files. The build fails on any
*new* warning code — the legacy backlog is frozen in
[`docs/warnings-baseline.md`](docs/warnings-baseline.md). Do not silence a new warning
by adding its code to that baseline; fix the warning.

Enable the pre-commit hook once per clone: `scripts/verify.ps1 -InstallHooks`.

## Design conventions
- API tests: keep arrange/act/assert in tests, move repeated setup into helpers/data providers.
- UI/mobile: use `Tests/Features -> Flows or step definitions -> Pages -> BasePage -> Driver/session`.
  Tests and flows express business intent only: they must not import browser-driver namespaces,
  define selectors, or call locator/evaluation APIs. Page objects own browser interaction and
  observable page state; assertion helpers own cross-page business validation. BDD/Gherkin is
  optional, but the page-object boundary is mandatory for every UI project.
  A test must not construct `PlaywrightBrowserSession` or access `session.Page`; put session
  lifetime, application navigation, retries, and multi-page orchestration in a named journey flow
  (or the established fixture when the project owns session lifetime there). Add the normal suite
  `Category` trait unless the scenario is deliberately explicit-only, and document that exclusion.
  Reuse the project's existing configuration and runner contract; do not add a second environment
  profile for one feature. See `docs/FRAMEWORK_ARCHITECTURE.md` for the concrete template.
- **Test data:** storage-table-driven via `RetrieveFromStorageTable` is the default — inline literals are reserved for negative paths, schema tests, and pure unit tests (full list in [`docs/TEST_DATA_STRATEGY.md`](docs/TEST_DATA_STRATEGY.md#when-you-genuinely-need-hard-coded-data)).
- **Response assertions:** for GraphQL endpoints use `EnsureGQLResponseSuccess()` — *not* `EnsureIsSuccessStatusCode()`. GraphQL returns HTTP 200 with an `errors` array on failure, so a status-only check passes false-positives. REST endpoints (including REST routes that live under `/Graphql/…` paths) keep `EnsureIsSuccessStatusCode()`. See the decision table in [`docs/TEST_DATA_STRATEGY.md` § Response assertions](docs/TEST_DATA_STRATEGY.md#response-assertions).
- **Response JSON navigation:** parse with `JObject.Parse(body)` (throws on malformed, returns non-null) and walk paths with `bookingResponse.RequireToken("booking.product[0].reference.pnrLocator")` from [`Core.Extensions.JObjectExtensions`](VAA.EAPI.Automation.Core/Extensions/JObjectExtensions.cs) — *not* `JsonConvert.DeserializeObject<JObject>` chained with `["…"]["…"]` indexers (CS8602 warnings + bare NRE on failure with no path info). Don't suppress with `!`. See [`docs/TEST_DATA_STRATEGY.md` § Navigating response JSON safely](docs/TEST_DATA_STRATEGY.md#navigating-response-json-safely).
- **JSON keys we author follow camelCase.** Two-letter acronyms mid-identifier capitalise only the first letter (`retrieveHhAccountRequest`, not `retrieveHHAccountRequest`); same rule as .NET's `IOException` → `ioException`. Three-letter+ acronyms follow the same rule (`xmlDocument`, not `xMLDocument`). This applies to any JSON payload we *build* in `Requests/Factory/*.cs`. JSON keys we *read* from a third-party response stay as the server emits them — match whatever the wire format actually uses.
- **Case-insensitive string compare:** use `source.Is(value)` from [`Core.Extensions.StringExtensions`](VAA.EAPI.Automation.Core/Extensions/StringExtensions.cs). It's `OrdinalIgnoreCase` under the hood with explicit null-safety on both sides, and it's the codebase's idiom — used everywhere for PNRs, FC numbers, env codes, partition keys etc. Don't reach for `string.Equals(other, StringComparison.OrdinalIgnoreCase)` directly when `.Is()` already wraps it.
- **Base URLs:** default to `Config.TestResourcesVirginAtlantic` (~367 call sites, customer-facing host). Only use `Config.DigitalTestVirginAtlantic` for `/digital/vred/*` and legacy raw `/digital/graphql` paths — and even then add a `_env.Is("prod")` fallback to `TestResourcesVirginAtlantic` because the digital-test host doesn't exist in prod. Don't be fooled by "Digital" in the config-class name: the `digital/...` segment of a URL like `testresources.virginatlantic.com/.../digital/Graphql/rest/v1/...` is path, not host. See [`docs/TEST_DATA_STRATEGY.md` § Base URLs](docs/TEST_DATA_STRATEGY.md#base-urls).
- **Don't use `#region` blocks to group a handful of lines.** Regions earn their keep on long files with several distinct sections (the storage-account helpers in `Core` are the bar — hundreds of lines, three concept buckets). For a typical test method, a single-line `// ...` comment above each step is enough and reads better in diffs and IDE folds.
- **Use explicit types, not `var`.** `Dictionary<string, string> pnrDetails`, `HttpResponseMessage response`, `string body` — never `var`. Explicit types make the contract a reader sees at-a-glance match what the compiler sees. The `csharp_style_var_*` rules in `.editorconfig` are at `warning` severity, so `scripts/verify.ps1`'s `dotnet format` check fails on any `var` in a **newly added** `.cs` file. Legacy files keep their `var` for now (a tracked burn-down — see [`docs/warnings-baseline.md`](docs/warnings-baseline.md)); the rule applies to new code immediately.
- Do not hard-code environment secrets in code or markdown docs.

## Documentation expectations
When changing behavior or execution patterns, update relevant docs under `VAA.EAPI.Automation/docs/`:
- `LOCAL_SETUP.md`
- `TEST_EXECUTION.md`
- `TEST_DATA_STRATEGY.md`
- `TROUBLESHOOTING.md`
- `CI_PIPELINE_MAP.md`
- `FRAMEWORK_ARCHITECTURE.md`
- `HARNESS_ENGINEERING.md`

## Definition of done
- `scripts/verify.ps1` passes (ratchet build + offline tests + formatting check).
- Build passes for impacted solution/project scope.
- Targeted tests (or clear reason for skip) are documented.
- Any data a test consumes is sourced from `RetrieveFromStorageTable` (or matches one of the documented exceptions in [`docs/TEST_DATA_STRATEGY.md`](docs/TEST_DATA_STRATEGY.md)).
- PR includes impacted project list and commands run.
- UI tests contain no direct Playwright/Selenium/Appium locator or driver calls; the architecture
  fitness test passes for the changed UI scope.
