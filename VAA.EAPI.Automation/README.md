# VAA.EAPI.Automation Solution Overview

## Purpose
The `VAA.EAPI.Automation.sln` solution contains automation test projects for backend microservices, GraphQL APIs, SOAP APIs, and Selenium-based front-end UI automation. It also includes shared core libraries and utility tooling to support the test suites.

## Solution Items
- `.editorconfig`
- `application.runsettings`
- `Assemblies/` (shared compiled dependencies referenced by tests)

## Project Inventory
### Core libraries
- `VAA.EAPI.Automation.Core` (`VAA.EAPI.Automation.Core/VAA.EAPI.Automation.Core.csproj`)
- `VAA.EAPI.Automation.Core.Delta` (`VAA.EAPI.Automation.Core.Delta/VAA.EAPI.Automation.Core.Delta.csproj`)
- `VAA.EAPI.Automation.Core.Playwright` (`VAA.EAPI.Automation.Core.Playwright/VAA.EAPI.Automation.Core.Playwright.csproj`)
- `VAA.EAPI.Automation.Core.UI` (`VAA.EAPI.Automation.Core.UI/VAA.EAPI.Automation.Core.UI.csproj`)

### Backend microservice and API test suites
- `VAA.EAPI.Automation.Book` (`VAA.EAPI.Automation.Book/VAA.EAPI.Automation.Book.csproj`)
- `VAA.EAPI.Automation.CRM` (`VAA.EAPI.Automation.CRM/VAA.EAPI.Automation.CRM.csproj`)
- `VAA.EAPI.Automation.FlightOperations` (`VAA.EAPI.Automation.FlightOperations/VAA.EAPI.Automation.FlightOperations.csproj`)
- `VAA.EAPI.Automation.MarketingLoyalty` (`VAA.EAPI.Automation.MarketingLoyalty/VAA.EAPI.Automation.MarketingLoyalty.csproj`)
- `VAA.EAPI.Automation.MyAccount` (`VAA.EAPI.Automation.MyAccount/VAA.EAPI.Automation.MyAccount.csproj`)
- `VAA.EAPI.Automation.NDC` (`VAA.EAPI.Automation.NDC/VAA.EAPI.Automation.NDC.csproj`)
- `VAA.EAPI.Automation.OCI` (`VAA.EAPI.Automation.OCI/VAA.EAPI.Automation.OCI.csproj`)
- `VAA.EAPI.Automation.OpenAI` (`VAA.EAPI.Automation.OpenAI/VAA.EAPI.Automation.OpenAI.csproj`)
- `VAA.EAPI.Automation.Search` (`VAA.EAPI.Automation.Search/VAA.EAPI.Automation.Search.csproj`)
- `VAA.EAPI.Automation.Trips` (`VAA.EAPI.Automation.Trips/VAA.EAPI.Automation.Trips.csproj`)
- `VAA.EAPI.Automation.Heimdall` (`VAA.EAPI.Heimdall/VAA.EAPI.Automation.Heimdall.csproj`)
- `VAA.EAPI.Automation.MobileApp` (`VAA.EAPI.Automation.MobileApp/VAA.EAPI.Automation.MobileApp.csproj`)
- `VAA.EAPI.Automation.StressTests` (`VAA.EAPI.Automation.StressTests/VAA.EAPI.Automation.StressTests.csproj`)

### Selenium UI automation
- `VAA.EAPI.Automation.MarketingLoyalty.UI` (`VAA.EAPI.Automation.MarketingLoyalty.UI/VAA.EAPI.Automation.MarketingLoyalty.UI.csproj`)
- `VAA.EAPI.Automation.MyAccount.UI` (`VAA.EAPI.Automation.MyAccount.UI/VAA.EAPI.Automation.MyAccount.UI.csproj`)

### Playwright UI automation
- `VAA.EAPI.Automation.InFlightApplications.UI` (`VAA.EAPI.Automation.InFlightApplications.UI/VAA.EAPI.Automation.InFlightApplications.UI.csproj`)
- `VAA.EAPI.Automation.Heimdall.UI` (`VAA.EAPI.Automation.Heimdall.UI/VAA.EAPI.Automation.Heimdall.UI.csproj`)
- `VAA.EAPI.Automation.SearchAndBook.UI` (`VAA.EAPI.Automation.SearchAndBook.UI/VAA.EAPI.Automation.SearchAndBook.UI.csproj`)

### Utilities and tooling
- `VAA.EAPI.Automation.Utility.DataProvisioning` (`VAA.EAPI.Automation.Utility.DataProvisioning/VAA.EAPI.Automation.Utility.DataProvisioning.csproj`)
- `VAA.EAPI.Automation.Utility.HttpMocker` (`VAA.EAPI.Automation.Utility.HttpMocker/VAA.EAPI.Automation.Utility.HttpMocker.csproj`)
- `VAA.EAPI.Automation.Utility.HttpMocker.Deployment` (`VAA.EAPI.Automation.Utility.HttpMocker.Deployment/VAA.EAPI.Automation.Utility.HttpMocker.Deployment.deployproj`)
- `VAA.EAPI.Automation.Utility.PNRCreation` (`VAA.EAPI.Automation.Utility.PNRCreation/VAA.EAPI.Automation.Utility.PNRCreation.csproj`)
- `VAA.EAPI.Automation.Utility.TestRunner.Service` (`VAA.EAPI.Automation.Utility.TestRunner.Service/VAA.EAPI.Automation.Utility.TestRunner.Service.csproj`)
- `VAA.EAPI.Automation.Utility.XsdValidation` (`VAA.EAPI.Automation.Utility.XsdValidation/VAA.EAPI.Automation.Utility.XsdValidation.csproj`)

## Notes
- The solution references pipeline YAML files under `VAA.EAPI.Pipelines` for test runner build and release automation.
- `Assemblies/` includes prebuilt DLLs used by some test projects.

## Core Features Covered by Automation
The automation projects validate integrations and workflows that commonly rely on platform services and shared tooling, including:
- Azure service connections used by the test runner and supporting utilities.
- Cosmos DB data interactions exercised by backend and API test suites.
- HTTP service virtualization via the HttpMocker utility.
- XML/XSD validation workflows used by SOAP-oriented scenarios.
- UI automation frameworks and page object utilities used by Selenium tests.

## Execution Flow (High-Level)
- Tests are executed via Azure DevOps pipelines using a centralized test runner, including CI/CD post-deployment validation of GraphQL and microservices.
- Environment configuration is applied via `local.settings.json` (gitignored) and pipeline variable groups.
- Supporting utilities such as HttpMocker are deployed as part of the test execution lifecycle.

## Running Tests Locally
When running tests locally, use Visual Studio Test Explorer and select the local settings file:
- Open the solution in Visual Studio and load `VAA.EAPI.Automation.sln`.
- Ensure `local.settings.json` is present locally (it is gitignored and provided out-of-band).
- Run the desired test suites from Test Explorer so the settings are applied.

For the Playwright In-Flight Applications suite, use its guarded runner scripts rather than an
unfiltered `dotnet test`. See
[`VAA.EAPI.Automation.InFlightApplications.UI/README.md`](VAA.EAPI.Automation.InFlightApplications.UI/README.md)
for one-time browser setup, project-local settings, safe regression execution, and the separately
authorised OTP and shared-account packs.

## Ownership and Contributions
- Each automation project is owned by the corresponding service team.
- Shared core libraries are maintained by the Automation Lead.
- New projects should follow existing naming and folder conventions.

## GraphQL End-to-End Booking Tests (VAA.EAPI.Automation.Book)

The E2E booking test suite in `VAA.EAPI.Automation.Book/Tests/NewBookFlow/GraphQL/EndtoEnd/` uses a single master test method driven by JSON-defined scenarios.

### File Structure

| File | Purpose |
|------|---------|
| `E2EEnums.cs` | Enums: `BookingType`, `PaymentMethod`, `SeatMode`, `InsuranceAction` |
| `E2EScenario.cs` | Scenario configuration class with JSON deserialization and `IXunitSerializable` |
| `E2ETestData.cs` | Loads scenarios from embedded JSON; provides filtered MemberData (`AllScenarios`, `SanityScenarios`, `RegressionScenarios`) |
| `E2ETestHelper.cs` | Flow step helpers (search, basket, insurance, seats, payment, order) |
| `E2EGraphQL.cs` | Test class with master `RunScenario` method and trait-filtered entry points |
| `e2e-scenarios.json` | All test scenarios in JSON (editable by QAs with intellisense) |
| `e2e-scenario-schema.json` | JSON Schema providing autocomplete, enum dropdowns, and conditional validation |

### Adding a New Test Scenario

Add a new entry to `e2e-scenarios.json`:
```json
{
  "testName": "MyNewTest_LHR_JFK",
  "origin": "LHR",
  "destination": "JFK",
  "journeyType": "Return",
  "seatMode": "AllSectors",
  "seatProductName": "Standard Seat",
  "cabinName": "VSCL",
  "insurance": "Accept"
}
```
The JSON Schema provides intellisense and validates required fields based on the scenario type (e.g. `seatProductName` is required when `seatMode` is not `None`).

### Pipeline Trait Filters

| Pipeline | Filter | Scenarios |
|----------|--------|-----------|
| Full regression | `Category=BookGraphQLRegression` | `RegressionEndToEndBooking` |
| Sanity | `Category=BookGraphQLPSSITSanity` | `SanityEndToEndBooking` |
| All | No filter / `FullyQualifiedName~EndToEndBooking` | All 78 scenarios |

### Modifying the Booking Flow

All booking flow logic is in `E2ETestHelper.cs`. Each step is a separate method (`PerformSearch`, `HandleInsurance`, `HandleSeatSelection`, `HandlePayment`, etc.). Changes to a flow step apply to all scenarios automatically.

## OCI PNR Creation Tests (VAA.EAPI.Automation.OCI)

The OCI PNR creation tests in `VAA.EAPI.Automation.OCI/Tests/PnrsCreation/` create booking PNRs for check-in testing.

### File Structure

| File | Purpose |
|------|---------|
| `OciRevenueFlowHelper.cs` | Shared booking flows for revenue scenarios (20 methods) |
| `OciRedemptionFlowHelper.cs` | Shared booking flow for redemption scenarios |
| `RevenuePNRCreationTest.cs` | Backend revenue tests → writes to `OCIPNRs` table |
| `RedemptionPNRCreationTest.cs` | Backend redemption tests → writes to `OCIPNRs` table |
| `MobileAppRevenuePNRCreation.cs` | Mobile app revenue tests → writes to `OnlineCheckInMobileAppData` table |
| `MobileAppRedemptionPNRCreation.cs` | Mobile app redemption tests → writes to `OnlineCheckInMobileAppData` table |

### Adding a New Data Consumer

To create PNR data for a new consumer (e.g. a new app or pipeline):

1. Create a new test class (e.g. `NewConsumerRevenuePNRCreation.cs`)
2. Use `OciRevenueFlowHelper` for the booking flow
3. Define `[Theory]` methods with your own `[Trait]` calling `_flow.Run*()` then writing to your target table
4. Add a storage method if needed in `WriteToAzureStorage` (or use `AddOciPnrsToStorageTable(config, "YourTableName")`)

### Pipeline Trait Filters

| Pipeline | Filter | Target Table |
|----------|--------|-------------|
| Backend OCI | `Category=Oci-Pnr-Creation` | `OCIPNRs` |
| Mobile App OCI | `Category=MobileApp-OciPnrCreation` | `OnlineCheckInMobileAppData` |

## Seat Selection Matching

The `FindAvailableSeat` methods in `GraphQLHelper.cs` use contains-based product name matching and strict `fareProduct` cabin filtering:
- `"Standard Seat"` matches `"Standard Seat Window"`, `"Standard Seat Aisle"`, `"Standard Seat NON BOOKED"`
- Cabin filter (`fareProduct`) is always an exact match — `VSUP` only matches Upper Class, `VSCL` only matches Economy Classic
- When `SeatProductName` is empty/null, any free seat is returned without filtering

## Loyalty Account Data Quality

The storage table retrieval methods (`RetrieveLoyaltyAccountNumberDetailsFromStorageTable`, `RetrieveCoBrandLoyaltyAccountNumberDetailsFromStorageTable`, `FetchLoyaltyAccountInfoExcludingPoints`) filter loyalty accounts to ensure the account holder is at least 18 years old based on their `DateOfBirth` field. This prevents test failures caused by under-age accounts being assigned as adult passengers.

## WiFi Portal BDD Tests (VAA.EAPI.Automation.MyAccount.UI)

The WiFi portal Selenium tests use a BDD (Behavior-Driven Development) approach with Gherkin feature files. QAs write test scenarios in plain English; developers maintain the step definitions.

### Architecture

```
Features/*.feature     (Gherkin scenarios — QA-editable)
   ↓
WifiPortalBddTests.cs  (xUnit runner with [FeatureFileData])
   ↓
WifiPortalSteps.cs     (Step definitions — [Given], [When], [Then])
   ↓
Page Objects           (WifiSignInPage, WifiSignUpPage, WifiOtpPage, WifiPasswordResetPage)
   ↓
WebDriverExtensions    (Element interactions, screenshots, accessibility)
   ↓
BrowserFactory         (Local Chrome or BrowserStack RemoteWebDriver)
```

### Feature Files

| Feature File | Scenarios |
|---|---|
| `SignIn.feature` | OTP sign in, email sign in, loyalty account sign in |
| `SignUp.feature` | Adult with OTP, Young Flyer skips OTP |
| `PasswordReset.feature` | Reset with OTP verification |
| `SignInValidation.feature` | Empty email, invalid email, empty password, account not found |
| `SignUpValidation.feature` | Empty first name, last name, date of birth |
| `PasswordResetValidation.feature` | Password length, number, uppercase, lowercase, special chars, mismatch, all rules met |

### Adding a New Test Scenario

Add a scenario to the appropriate `.feature` file using existing step phrases:
```gherkin
Scenario: Sign in with new flow
    Given I accept the cookie banner on the WiFi portal
    And I have a Gold loyalty account
    When I enter my email and password
    And I click Log In
    Then I should see the connected message
```

If a new step is needed, add a method to `WifiPortalSteps.cs` with the matching `[Given]`, `[When]`, `[And]`, or `[Then]` attribute.

### Built-in Features

- **ExtentReports** — screenshots at checkpoints, failure diagnostics with console/network errors
- **Accessibility** — axe-core scans at page loads, violations logged as info in ExtentReport
- **BrowserStack** — cross-browser/device testing when `BROWSERSTACK_USERNAME` is configured; falls back to local headless Chrome otherwise

### Running

```bash
# All WiFi portal BDD tests
dotnet test VAA.EAPI.Automation.MyAccount.UI --filter "Category=WifiPortalBDD"

# Specific feature
dotnet test VAA.EAPI.Automation.MyAccount.UI --filter "DisplayName~SignIn"
```

## Trips seat scenario test data notes
- Seat-selection test data for mobile GraphQL journeys is stored in Azure Table Storage table `MobileAppPnrsForSeatScenarios`.
- Trips GraphQL seat tests now include award-booking coverage using reservation type `AWARDBOOKING`, alongside existing revenue seat scenarios.
- Relevant trait filters in `VAA.EAPI.Automation.Trips` include `SelectFreeSeats-Regression`, `SelectPaidSeats-Regression`, and `SelectSeatsAwardBooking-Regression`.

[![Trips PNR Creation](https://github.com/VAAD1ST/eapi/actions/workflows/qa-trips-pnr-creation.yml/badge.svg)](https://github.com/VAAD1ST/eapi/actions/workflows/qa-trips-pnr-creation.yml)
