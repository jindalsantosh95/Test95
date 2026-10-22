# UI/Mobile Automation Patterns (VAA.EAPI.Automation)

## Layering & Responsibilities

Follow the existing UI automation layering:

```
Feature Files (.feature)
   ↓
xUnit Test Classes
   ↓
Step Definitions
   ↓
Page Objects
   ↓
BasePage (locators, waits, scrolling)
   ↓
Driver (Local / BrowserStack)
```

Keep each layer focused on a single responsibility to maintain readability and stability. Gherkin
and step definitions are optional; the page-object boundary is not.

For direct xUnit/Playwright suites use:

```text
xUnit Tests → Journey Flows → Page/Component Objects → BasePage → Driver/session
                         ↘ Assertion helpers
```

Tests and flows must not import the browser-driver namespace, contain selectors, call locator APIs,
evaluate DOM scripts or pause the driver. `Pages/` owns interactions and observable page state;
`Assertions/` owns validation spanning multiple pages or captured models.

## Feature Files

- Express user intent and behavior in Gherkin.
- Keep steps declarative (what the user does, not how the UI works).

## xUnit Test Classes

- Bind a feature file and scenario to a test method.
- Use the existing fixture to manage the Appium session.

Example pattern:

```csharp
[Theory]
[Trait("Category", "BDD")]
[FeatureFileData<SignInSteps>(
    "Features/Accounts/SignIn.feature",
    "Successful Login with Dynamic Credentials")]
public async Task Run_Login_Feature(Scenario scenario)
{
    await scenario.Run(_fx, _output);
}
```

## Step Definitions

- Map Gherkin steps to intent-based page object calls.
- Avoid direct locators or XPath in step definitions.
- Keep assertions in the step definitions or dedicated assertion helpers.

Example pattern:

```csharp
[When("I navigate to the Sign In screen")]
public void NavigateToSignIn()
{
    _signInPage.OpenSignIn();
}
```

## Page Objects

- Expose public intent-based methods that hide locator details.
- Inherit from `BasePage` and use its protected primitives.
- Use key-based locator strategy instead of hard-coded locators in steps.
- Inherit browser-backed page objects from the suite's `BasePage`.
- Prefer component objects for independently reusable modals, widgets and hosted iframes.
- Do not put whole journeys or cross-page business assertions into a page object.

Example pattern:

```csharp
public sealed class LandingPage : BasePage
{
    public void RejectCookiesIfShown()
        => TapByKeyIfFound("Reject All");
}
```

## Web UI BDD (MyAccount.UI — WiFi Portal)

The WiFi portal tests follow the same BDD pattern but use Selenium `IWebDriver` via `BrowserFactory` instead of Appium:

```
Features/WifiPortal/*.feature     (Gherkin scenarios)
   ↓
WifiPortalBddTests.cs            ([Theory] + [FeatureFileData<WifiPortalSteps>])
   ↓
WifiPortalSteps.cs               ([Given], [When], [And], [Then] step bindings)
   ↓
Page Objects                      (WifiSignInPage, WifiSignUpPage, WifiOtpPage, WifiPasswordResetPage)
   ↓
WebDriverExtensions               (Element interactions, screenshots, accessibility)
   ↓
BrowserFactory                    (Local Chrome or BrowserStack RemoteWebDriver)
```

Additional capabilities in the web UI steps:
- **ExtentReports** — `_logger` passed to all page object calls; screenshots at checkpoints
- **Accessibility** — `_webDriver.CheckAccessibility(_logger)` after key page loads; uses axe-core
- **BrowserStack** — auto-detected via `BROWSERSTACK_USERNAME` config; local Chrome fallback

Example web UI step definition:

```csharp
[Given("I accept the cookie banner on the WiFi portal")]
public void AcceptCookieBanner()
{
    _signInPage.ClickYesIAgree(_logger);
    _webDriver.LogScreenshot(_logger, "Landing page loaded");
    _webDriver.CheckAccessibility(_logger);
}
```

Example web UI test runner:

```csharp
[Theory]
[Trait("Category", "WifiPortalBDD")]
[FeatureFileData<WifiPortalSteps>("Features/WifiPortal/SignIn.feature")]
public async Task SignIn(Scenario scenario)
{
    await scenario.Run(scenario.Feature.Name, scenario.Name, _reportFixture, _memoryStore, _otpTableService);
}
```

## When Adding New Screens or Flows

- Add or update page objects under `PageObjects/`.
- Keep page object methods short and focused.
- Reuse existing base page helpers (scrolling, waits, tap helpers) rather than re-implementing.
- Update feature files and test classes to point at the new scenario.
- For web UI: add element locators to `ElementsLibrary/`, page methods to `PageObjects/`, step bindings to `StepDefinitions/`, and scenarios to `Features/`.
