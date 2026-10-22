# Framework Architecture (VAA.EAPI.Automation)

This document describes the high-level architecture and layering used across automation projects in `VAA.EAPI.Automation.sln`.

## Solution-Level Building Blocks

- **Core libraries**: shared utilities, framework primitives, and cross-cutting helpers.
- **Domain test suites**: project-specific API/GraphQL/SOAP automation.
- **UI/mobile suites**: page objects, step definitions, and scenario orchestration.
- **Utilities/tooling**: supporting tools (e.g., mockers, data utilities, runners).

## API/GraphQL Automation Layering

Typical structure:

1. Test classes
2. Fixtures / setup
3. Request helpers / endpoint wrappers
4. Shared helpers / data providers
5. Assertions in test scope

Guidance:

- Keep HTTP and serialization details in helper layers where possible.
- Keep test methods focused on business intent and assertions.

## UI/Mobile Automation Layering

Typical structure:

1. Feature files or xUnit scenario definitions
2. Thin tests / step definitions
3. Reusable journey flows (when behaviour spans pages)
4. Page objects and component objects
5. Base page primitives
6. Driver/session infrastructure
7. Assertion helpers for cross-page business outcomes

Guidance:

- Keep step definitions thin.
- Keep locator details in page objects/base page.
- Reuse common waits/scroll/tap primitives.
- Do not import Playwright, Selenium, or Appium driver APIs in functional test classes.
- Tests describe the scenario and assert the outcome. Flows coordinate page objects but do not
  contain selectors or browser scripts. Page objects expose intent-based actions and observable
  state; they do not own cross-page business assertions.
- BDD is an optional authoring style. Page Object Model boundaries apply equally to direct xUnit tests.

### Required UI project structure

```text
Tests/ or Features/     scenario intent and outcome
Flows/                  reusable multi-page orchestration
Pages/                  selectors, interactions and page/component state
Assertions/             cross-page business assertions
Infrastructure/         browser/session creation and reporting
```

`BasePage` is the inheritance boundary for browser-backed page objects. A page object may compose
component objects when a reusable widget, iframe, header or modal has its own lifecycle. Do not
create a single application-wide page object or move complete journeys into a page class.

The architecture fitness suite scans UI functional tests and rejects direct browser-driver imports,
locator calls, DOM evaluation and interactive pauses. The isolated setup/connectivity test folder is
the narrow exception because its purpose is to verify the driver itself.

### Journey flow implementation template

For a multi-page UI journey, the xUnit test selects data, invokes one intent-based flow method and
asserts the returned business outcome. It does not construct a browser session, access `Page`, open
the application shell, repeat failure-reporting boilerplate, or read feature-specific environment
variables directly.

```text
Journey test
  -> RewardFlightFinderFlow.RunAsync(scenario)
      -> ApplicationPage.NavigateAsync()
      -> RewardFlightFinderPage.SearchAsync(criteria)
      -> RewardFlightFinderPage.ReadAvailabilityAsync()
  -> RewardFlightFinderAssertions.MatchExpectedAvailability(result)
```

The flow owns session lifetime and multi-page sequencing; pages own selectors, waits, native-control
selection and observable state; assertion helpers own business comparisons. Prefer an established
project fixture when it already owns session lifetime. Extend the existing project configuration and
runner contract instead of creating a feature-specific environment profile.

Every new journey must also declare how it runs: add the project's normal regression `Category`
trait, or use the project's documented explicit-only trait and record why it is intentionally not in
regression. Required settings must appear in the project example settings and in the workflow or
suite manifest that executes the category. Before opening a PR, run `scripts/verify.ps1`; its UI
automation sensor rejects newly added tests that reach through the flow boundary.

### Shared Playwright accessibility scanning

`Core.Playwright/Accessibility/PlaywrightAccessibilityScanner` is the common axe-core integration
for Playwright UI suites. It injects the centrally versioned `Deque.AxeCore.Commons` bundle and
returns structured violations for WCAG 2.0 A/AA, WCAG 2.1 A/AA and WCAG 2.2 AA tags. Page objects invoke the
scanner because it operates on the active Playwright page; tests and flows remain free of browser
driver APIs.

The scanner reports evidence rather than enforcing one global policy. Each domain suite owns its
assertion or reporting threshold so it can introduce accessibility gates incrementally without
forking the axe integration.

### Creating a Playwright UI project

Do not use an older UI suite as an informal template. From `VAA.EAPI.Automation/`, run the
repository scaffolder so every new project starts with the required boundaries and shared session:

```pwsh
pwsh -NoProfile -File skills/new-playwright-ui-project/scripts/new-playwright-ui-project.ps1 `
  -DomainName <PascalCaseDomain> `
  -Description "<business purpose>"
```

The generator adds the project to the solution but deliberately does not restore packages, install
browsers, run live tests, or modify source control. Follow the generated README, then run the
architecture fitness tests before exercising an environment.

## Web UI BDD Automation (MyAccount.UI — WiFi Portal)

The WiFi portal tests follow the same BDD layering as mobile but use Selenium `IWebDriver` instead of Appium:

1. Feature files (`.feature`) — Gherkin scenarios in `Features/WifiPortal/`
2. xUnit test class (`WifiPortalBddTests.cs`) — `[Theory]` + `[FeatureFileData<WifiPortalSteps>]`
3. Step definitions (`WifiPortalSteps.cs`) — `[Given]`, `[When]`, `[And]`, `[Then]` attributes
4. Page objects (`WifiSignInPage`, `WifiSignUpPage`, `WifiOtpPage`, `WifiPasswordResetPage`)
5. Element libraries (`WifiSignIn`, `WifiSignUp`, `WifiOtp`, `WifiPasswordReset`)
6. Driver infrastructure (`BrowserFactory` — local Chrome or BrowserStack `RemoteWebDriver`)

Additional built-in capabilities:
- **ExtentReports** — screenshots at checkpoints, failure diagnostics (console errors, network errors)
- **Accessibility** — axe-core scans at page loads via `CheckAccessibility`, violations logged as info
- **BrowserStack** — auto-detected via `BROWSERSTACK_USERNAME` config; falls back to local headless Chrome

The custom BDD framework in `Core.UI/Xunit/Features/` (attributes, `FeatureFileDataAttribute`, `ScenarioRunner`) is shared between mobile and web UI projects.

### Shared Hosted Payment Page automation

Direct Hosted Payment Page automation is shared across domain suites rather than owned by Book. `Core.Playwright/HostedPayment` owns Playwright locators and payment-provider interaction, while `Core.UI/Helpers/HostedPaymentAutomation` provides the common card/PayPal facade, required HPP headers and Selenium fallback.

Book can select Playwright per E2E scenario. Trips and OCI PNR-data-creation jobs select it through the `UsePlaywrightForHpp` runtime setting. This keeps driver selection outside test bodies and allows a configuration-only rollback to Selenium.

## Cross-Cutting Concerns

- Configuration management and environment selection.
- Shared logging/assertion conventions.
- Trait/category naming for execution and reporting.

## Scheduled Suite Onboarding

Ordinary API and GraphQL automation projects use a three-layer CI model:

1. `config/automation-suites/<slug>.json` declares the suite contract.
2. A thin `.github/workflows/qa-<slug>.yml` owns GitHub triggers and secret mapping.
3. `.github/workflows/qa-run-automation-suite.yml` validates the manifest and delegates execution
   to the shared regression runner.

Create both contract files with `scripts/new-automation-suite.ps1`. This keeps operational
settings machine-readable for agents and prevents build, reporting, artifact, notification and
runner-cleanup logic from being copied into each new workflow. Dedicated workflows remain valid
for UI browser matrices, multi-stage provisioning, or other execution models the shared runner
cannot express.

## Change Placement Rules

When adding functionality:

- Add project-specific behavior inside the target project first.
- Promote to shared core only when reused by multiple projects.
- Update docs when introducing new framework primitives.

## Native Mobile Automation (MobileApp.UI)

Native mobile journeys use direct xUnit tests rather than feature files:

1. xUnit journey test — business intent and assertions.
2. Journey orchestration — reusable multi-page flow.
3. Appium page objects — intent-based actions and locators.
4. AppBasePage — waits, accessibility-ID lookup, scrolling and gestures.
5. MobileFixture and driver factories — local Appium or BrowserStack session lifecycle.

The mobile UI project depends only on Core.UI and Core. It must not reference backend
domain test projects; shared data or diagnostics belong in an appropriate Core abstraction.

Native controls are located by stable accessibility ID. Page objects wait for observable
UI state rather than elapsed time, and normal interactions must not use device coordinates.
Reusable handlers own cross-journey app entry behaviour. For example, `AppEntryHandler`
uses `OnboardingPage` to complete `Get started` -> `Continue` -> `Maybe later`, then enters
as a guest through `WelcomePage` and delegates optional consent dismissal to
`PrivacyBannerPage`. See the mobile locator and interaction guardrails in
`docs/principles/test-design-principles.md`.
