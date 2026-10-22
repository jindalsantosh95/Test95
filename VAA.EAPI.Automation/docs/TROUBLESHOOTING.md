# Troubleshooting (VAA.EAPI.Automation)

Common issues and quick triage steps for automation development.

## Direct HPP browser failures in Book GraphQL

The core regression workflow defaults scheduled and manual runs to Playwright for every supported HPP journey. Selecting the manual `scenario` mode instead honours each row's `usePlaywrightForHpp` value, while selecting `selenium` provides a full rollback path. The direct HPP opens outside the frontend iframe, launches the installed Chrome channel, and applies the same request headers as the existing Selenium path. Co-brand/Synchrony remains on Selenium because its external application endpoint is not currently reliable; an experimental Playwright HPP adapter is retained for later validation.

If a migrated journey fails before the payment form is displayed, confirm Chrome is installed on the runner and that the HPP routing headers are accepted. Do not print their values or the full HPP URL. To compare individual scenario routing, run manually with `book_hpp_mode=scenario` and remove or set `usePlaywrightForHpp` to `false` on that scenario. Use `book_hpp_mode=selenium` to roll back the whole run.

For a local visible-browser check, set `BookHppPlaywrightHeadless=false` before running the single pilot scenario. CI remains headless by default.

If Chrome reports that the computer has run out of resources, check that `HPP_PLAYWRIGHT_MAX_CONCURRENT_SESSIONS` is present and set to `2` on the shared Windows runner. The shared HPP facade queues additional payment interactions rather than launching an unbounded number of Chrome processes. A `TargetClosedException` affecting many scenarios in the same run is commonly a cascade from this Chrome launch failure, rather than evidence that every HPP journey is functionally broken.

## 1) Build Fails on Restore/Compile

### Symptoms
- NuGet restore errors.
- Missing assembly/reference failures.

### Checks
- Run restore/build from `VAA.EAPI.Automation` folder.
- Confirm SDK version compatibility.
- Confirm internal package/source access.

## 2) Test Fails Due to Configuration

### Symptoms
- Null/empty endpoint values.
- Authentication initialization failures.

### Checks
- Validate `local.settings.json` presence and required keys.
- Confirm environment values and credentials are up-to-date.

## 3) Environment/Connectivity Failures

### Symptoms
- DNS resolution errors.
- TLS/connect timeout errors.

### Checks
- Verify network/VPN access.
- Re-run against a known healthy environment.

## 4) Flaky API Assertions

### Symptoms
- Intermittent response shape differences.
- Timing-sensitive status transitions.

### Checks
- Verify data setup and fixture isolation.
- Add polling/retry only where behavior is eventually consistent.
- Assert stable fields first, then optional/non-deterministic fields.

## 5) UI/Mobile Automation Instability

### Symptoms
- Element not found / stale element.
- Session startup or capability errors.

### Checks
- Run `scripts/setup-mobile-app-ui.ps1` first. It validates the selected `RunMode` and names each
  missing property in `VAA.EAPI.Automation.MobileApp.UI/local.settings.local.json` without
  printing secrets.
- Use `RunMode=Remote` with the `BrowserStack` block, or `RunMode=Local` with `Platform=iOS` and
  the `Appium.iOS` block. Do not move BrowserStack credentials into `appsettings.json`.
- Ensure page object locators are current.
- Use base page waits/scroll helpers rather than ad-hoc sleeps.
- Validate driver/capability configuration for the target platform.
- For Search and Book Playwright runs, inspect `browserstack-preflight.json` in each shard artifact.
  Compare `egressIp` between passing and failing sessions, and confirm `frontendAccessible` is
  `true`. A failed preflight stops the scenario before business-flow assertions and retains
  `failure.png` for access-page evidence.
- For a Search and Book failure, first use the `Mobile BrowserStack failure evidence` block
  in the xUnit/TRX output to find the exact BrowserStack session, build and capture profile.
- In BrowserStack, use the explicit session status and reason reported by the harness.
  `CLIENT_STOPPED_SESSION` only means normal WebDriver cleanup called `Quit()`; it is not a
  pass/fail result. Sessions should be named `TestClass | TestMethod`. A generic name indicates
  the test ended before the shared Search and Book wrapper started and should be treated as a
  setup/infrastructure defect.
- If the block reports that GraphQL response capture was disabled, that completed session's
  response content cannot be recovered. Enable `BrowserStack.NetworkLogs` and
  `BrowserStack.CaptureNetworkContent` before the next session.
- On payment confirmation failure, the functional test fails immediately when the app displays
  `Try secure payment again`. The original failing BrowserStack session reports a sanitised
  backend summary; neither payment nor the test is retried.
- Never paste a raw HAR, GraphQL body, query variables or backend error message into test output.

## 6) Pipeline vs Local Differences

### Symptoms
- Passes locally, fails in CI/CD.

### Checks
- Compare environment variables and secret availability.
- Compare test filters/traits between local and pipeline.
- Confirm pipeline agent has required dependencies.

## Escalation Checklist

Before escalating an issue, capture:

- Exact command used.
- Project and test/trait impacted.
- Failing assertion or stack trace excerpt.
- Environment name and timestamp.
