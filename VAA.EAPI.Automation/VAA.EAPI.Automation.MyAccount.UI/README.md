# VAA.EAPI.Automation.MyAccount.UI

Selenium-based BDD tests for the WiFi portal (Starlink in-flight connectivity sign in, sign up, password reset).

## Architecture

```
Features/WifiPortal/*.feature     Gherkin scenarios (QA-editable)
   |
WifiPortalBddTests.cs            xUnit runner ([Theory] + [FeatureFileData])
   |
WifiPortalSteps.cs               Step definitions ([Given], [When], [And], [Then])
   |
Page Objects                      WifiSignInPage, WifiSignUpPage, WifiOtpPage, WifiPasswordResetPage
   |
Element Libraries                 WifiSignIn, WifiSignUp, WifiOtp, WifiPasswordReset
   |
WebDriverExtensions               Element interactions, screenshots, accessibility
   |
BrowserFactory                    Local Chrome or BrowserStack RemoteWebDriver
```

## Feature Files

| File | Scenarios |
|---|---|
| `SignIn.feature` | OTP sign in, email sign in, loyalty account sign in |
| `SignUp.feature` | Adult with OTP, Young Flyer skips OTP |
| `PasswordReset.feature` | Reset with OTP verification |
| `SignInValidation.feature` | Empty/invalid email, empty password, account not found |
| `SignUpValidation.feature` | Empty first name, last name, date of birth |
| `PasswordResetValidation.feature` | Length, number, uppercase, lowercase, special chars, mismatch, all rules met |

## Adding a New Scenario

Write a scenario in the appropriate `.feature` file using existing step phrases:

```gherkin
Scenario: Sign in with new flow
    Given I accept the cookie banner on the WiFi portal
    And I have a Gold loyalty account
    When I enter my email and password
    And I click Log In
    Then I should see the connected message
```

If a new step is needed, add a method to `WifiPortalSteps.cs`:

```csharp
[When("I do something new")]
public void DoSomethingNew()
{
    _signInPage.SomeMethod(_logger);
    _webDriver.LogScreenshot(_logger, "Description");
}
```

## Built-in Features

- **ExtentReports** -- screenshots at key checkpoints, failure diagnostics (console errors, network errors, page source)
- **Accessibility** -- axe-core scans after page loads via `CheckAccessibility`, violations logged as info (tests still pass)
- **BrowserStack** -- cross-browser/device testing when `BROWSERSTACK_USERNAME` is configured; local headless Chrome otherwise

## Running Tests

```bash
# All WiFi portal BDD tests
dotnet test VAA.EAPI.Automation.MyAccount.UI --filter "Category=WifiPortalBDD"

# Specific feature
dotnet test VAA.EAPI.Automation.MyAccount.UI --filter "DisplayName~SignIn"

# Password reset validation only
dotnet test VAA.EAPI.Automation.MyAccount.UI --filter "DisplayName~PasswordResetValidation"
```

## Local Setup

1. Ensure `local.settings.local.json` is present with required config values
2. `dotnet restore` and `dotnet build`
3. Run from Test Explorer or CLI

## BrowserStack Setup

Add to your config (`local.settings.local.json` or environment variables):

| Key | Required | Default |
|-----|----------|---------|
| `BROWSERSTACK_USERNAME` | Yes | -- |
| `BROWSERSTACK_ACCESS_KEY` | Yes | -- |
| `BROWSERSTACK_PROJECT` | No | "EAPI Automation" |
| `BROWSERSTACK_BUILD` | No | "Build-{timestamp}" |

When `BROWSERSTACK_USERNAME` is not set, tests run on local headless Chrome.

## Project Structure

```
VAA.EAPI.Automation.MyAccount.UI/
  Constants/
    WifiPortal.cs                    Base URL configuration
  ElementsLibrary/
    WifiSignIn.cs                    Sign in page locators
    WifiSignUp.cs                    Sign up page locators
    WifiOtp.cs                       OTP page locators
    WifiPasswordReset.cs             Password reset page locators
  PageObjects/
    WifiSignInPage.cs                Sign in page interactions
    WifiSignUpPage.cs                Sign up page interactions
    WifiOtpPage.cs                   OTP page interactions
    WifiPasswordResetPage.cs         Password reset page interactions
  Features/WifiPortal/
    SignIn.feature                    Sign in Gherkin scenarios
    SignUp.feature                    Sign up Gherkin scenarios
    PasswordReset.feature            Password reset Gherkin scenarios
    SignInValidation.feature          Sign in validation scenarios
    SignUpValidation.feature          Sign up validation scenarios
    PasswordResetValidation.feature   Password reset validation scenarios
  StepDefinitions/WifiPortal/
    WifiPortalSteps.cs               All step definitions
  Tests/
    WifiPortalBddTests.cs            xUnit test runner
```

## Coding Standards

- Page object methods accept `ExtentTest logger` (nullable) as first parameter
- Use `logger?.Log()` for null safety (BDD steps pass the logger, but it must be optional)
- Element locators go in `ElementsLibrary/`, not in page objects or step definitions
- Step definitions should be thin -- delegate to page objects for all UI interaction
- Use `WebDriverExtensions` methods (`FindElement`, `FindElementAndClick`, etc.) for all element interactions
