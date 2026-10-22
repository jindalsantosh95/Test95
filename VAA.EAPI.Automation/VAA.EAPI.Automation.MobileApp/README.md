# VAA.EAPI.Automation

## Prerequisites

1. **BrowserStack Credentials**:
   - Set your BrowserStack credentials in `local.settings.local.json` (do not commit this file):
     ```json
     {
       "BROWSERSTACK_USERNAME": "<your_browserstack_username>",
       "BROWSERSTACK_ACCESS_KEY": "<your_browserstack_access_key>"
     }
     ```
   - Alternatively, you can export them as environment variables if preferred.

2. **App Settings**:
   - Update `appsettings.json` and `local.settings.json` for your environment. These files are gitignored and should not be committed.

3. **Dependencies**:
   - Restore dependencies:
     ```bash
     dotnet restore
     ```

4. **Appium Server**:
   - Start Appium before running any tests:
     ```bash
     appium
     ```
   - Or use Appium Desktop and ensure it is running on the default port (4723).

## BDD + Gherkin (xUnit-Owned Execution)

### Purpose
We use Gherkin feature files for business-readable scenarios and traceability. Test execution is owned by xUnit (via our custom ParallelXunitTestFramework). Gherkin is not the runtime runner.

### Architecture Overview
- **Feature Layer**: `.feature` files describe business scenarios in plain language.
- **Step Definition Layer**: C# step definition classes map Gherkin steps to automation calls. Step definitions are thin and orchestrate calls to core logic only.
- **Core Logic Layer**: All UI/data logic lives in MobileActions, Helpers, and Flows. This includes Appium interaction, retries, platform handling, and data access.
- **xUnit**: Controls test execution, parallelism, and ordering (only for edge cases).

### Key Rules
- Feature files must use business language, not UI selectors or implementation details.
- Step definitions must not contain selectors, platform branching, or direct driver/data access. Only call helpers/flows.
- Core logic layer owns all UI/data logic, retries, and platform specifics.
- ScenarioContext is used for per-scenario data (no static shared state).
- Tests should be order-independent. Use ordering only for documented edge cases.
- Any change to a feature file must be matched by step definition and flow/helper updates.

### Example (Good)
```
Given I have generated fresh credentials
When I sign in
Then I should see the dashboard
```

### Example (Avoid)
```
When I tap element 'btn_login'
Then I should see XPath ...
```

### Reporting & Artifacts
- xUnit is the execution engine.
- BrowserStack provides session-level artifacts (video, logs, etc.).
- The framework logs feature/scenario names, session URLs, step boundaries, and failure artifacts (screenshots, logs).

## Running Tests

To run tests, use the following commands:

### Run a specific BDD test (step definition class)
```bash
# Example: Run AddATrip BDD test
$ dotnet test --filter "AddATrip" --logger "console;verbosity=detailed"

# Example: Run LoginSteps BDD test
$ dotnet test --filter "LoginSteps" --logger "console;verbosity=detailed"
```

### Run all tests
```bash
$ dotnet test VAA.EAPI.Automation.MobileApp
```

### Build the project
```bash
$ dotnet build VAA.EAPI.Automation.MobileApp
```

### Clean and rebuild
```bash
$ dotnet clean VAA.EAPI.Automation.MobileApp && dotnet build VAA.EAPI.Automation.MobileApp
```

## App Settings Configuration

The `appsettings.json` file controls the test run mode and platform. Example:

- **iOS**:
  ```json
  "RunMode": "Local",
  "Platform": "iOS",
  "Appium": {
    "iOS": {
      "App": "/path/to/your/ios/app"
    }
  }
  ```
- **Android**:
  ```json
  "RunMode": "Local",
  "Platform": "Android",
  "Appium": {
    "Android": {
      "App": "/path/to/your/android/app.apk"
    }
  }
  ```
- **BrowserStack**:
  ```json
  "RunMode": "Remote",
  "BrowserStack": {
    "Platform": "iOS" // or "Android"
  }
  ```

> **Note:**
> - Update the `App` path for iOS/Android as per your local build.
> - For BrowserStack, use the app URL provided after upload.
> - Never commit sensitive credentials or local app paths.

## Project Structure

- **Core.UI**: Driver factories for local/remote execution.
- **MobileApp**: Test cases and supporting files for mobile automation.
- **Features**: BDD feature files.
- **StepDefinitions**: Step definitions for BDD tests.
- **Helpers/Flows**: All UI/data logic, retries, and platform handling.

## Troubleshooting

- If tests fail to locate elements, check accessibility identifiers in the app build.
- For BrowserStack issues, verify app upload and platform config.
- For Android, ensure emulator is running and APK is installed.
- For iOS, ensure simulator is running and .app is built for simulator.

## Contribution

- Follow the standard branching strategy.
- Ensure all tests pass locally before raising a pull request.
- Add documentation for any new features or changes.

---

For any issues, contact the QA Automation team.
