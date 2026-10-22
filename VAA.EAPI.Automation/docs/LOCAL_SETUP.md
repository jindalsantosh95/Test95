# Local Setup Guide (VAA.EAPI.Automation)

This document helps automation engineers set up and run automation projects from `VAA.EAPI.Automation.sln` locally.

## 1) Prerequisites

- .NET SDK compatible with the solution projects.
- Visual Studio (recommended for Test Explorer workflows) or CLI tooling.
- Access to required environment secrets and test infrastructure.
- Local copy of `local.settings.json` if required by target suites (gitignored).

## 2) Open the Solution

- Open `VAA.EAPI.Automation/VAA.EAPI.Automation.sln` in Visual Studio, or
- Use CLI from the solution folder.

## 3) Restore & Build

```bash
cd VAA.EAPI.Automation
dotnet restore VAA.EAPI.Automation.sln
dotnet build VAA.EAPI.Automation.sln -c Release
```

## 4) Local Configuration

- Ensure `local.settings.json` (or equivalent local config) is present for suites that require it.
- Avoid committing secrets; use local files, secret stores, or pipeline variable groups.
- Confirm environment endpoint values before running integration-heavy suites.

## 5) Running Tests

Preferred local workflow (Visual Studio):

1. Open Test Explorer.
2. Select the target project and trait/category.
3. Run relevant subsets first (smoke/feature-focused), then broader validation.

CLI examples are in `TEST_EXECUTION.md`.

## 6) Common Setup Issues

### Missing local settings

Symptoms:
- Tests fail early with configuration/null endpoint errors.

Actions:
- Verify `local.settings.json` exists and is valid for your target suite.
- Confirm required keys are present for that domain.

### Environment access failures

Symptoms:
- Auth failures, DNS errors, or timeouts.

Actions:
- Validate VPN/network access.
- Confirm environment endpoint and credentials.

### UI/mobile driver startup issues

Symptoms:
- Driver/session creation fails before test flow starts.

Actions:
- Verify local driver capabilities and runtime dependencies.
- Re-run a known-good smoke test in the same project.

## 7) First Contribution Quick Check

One-time, after cloning, enable the harness pre-commit hook:

```pwsh
scripts/verify.ps1 -InstallHooks
```

Before opening your first PR, run the harness verification from the solution folder:

```pwsh
scripts/verify.ps1
```

It runs the analyzer ratchet build, the offline test suites (architecture + `Core`
behavior), and a formatting check. See [`HARNESS_ENGINEERING.md`](HARNESS_ENGINEERING.md)
("Using the harness") for the full picture. Also ensure:

- Build succeeds for the solution.
- Targeted tests for your changed project pass.
- Docs/README updates are included if behavior or setup changed.

## Native Mobile BrowserStack Configuration

VAA.EAPI.Automation.MobileApp.UI uses Appium with BrowserStack for iOS execution from
Windows, macOS or Linux. Copy `local.settings.local.example.json` to
`local.settings.local.json`. In that one gitignored file, set `RunMode` to `Remote` and replace
the `BrowserStack.Username`, `BrowserStack.AccessKey`, and `BrowserStack.App` placeholders.
`BrowserStack.App` must be the immutable `bs://...` ID returned by the app upload.

The same file contains the `Appium.iOS` block for macOS local execution. Change `RunMode` to
`Local` to use it; the unused BrowserStack block can remain. The committed example contains only
placeholders. Do not commit the populated local file, BrowserStack credentials, or payment data.
Keep `Appium.iOS.ForceAppLaunch` set to `true`, `NoReset` set to `false`, and `FullReset` set to
`false`. This relaunches the installed app with the configured feature arguments for each test
without using `clearApp` or uninstalling it. Refinement tests normalise their own sort/filter
state through accessibility-ID-driven UI actions.

For a guided setup, run `scripts/setup-mobile-app-ui.ps1` from `VAA.EAPI.Automation`, or ask
Codex to "setup the mobile QA tests". The command creates a gitignored settings file from
`VAA.EAPI.Automation.MobileApp.UI/local.settings.local.example.json`, checks tooling and
configuration, and runs safe offline tests. Its first run may stop after creating the file and
list placeholders to replace. Edit the file and rerun the same command; it then runs only the
launch/connectivity test for the selected `RunMode`.
It does not run booking or payment journeys. Use `-SkipConnectivityTest` for offline-only
validation.

### Mobile platform support

- **Windows, macOS and Linux to BrowserStack iOS:** supported. Install PowerShell 7 and the
  .NET 10 SDK, then provide BrowserStack credentials and the uploaded iOS app's `bs://` ID.
  Xcode and a locally installed Appium server are not required for BrowserStack execution.
- **Local iOS simulator or device:** macOS-only. Xcode, Appium and the XCUITest driver must
  be installed and the local Appium capabilities configured.
- **Local iOS from Windows or Linux:** not supported because the required Apple tooling is
  macOS-only.
- **Android:** the shared driver factory supports local and BrowserStack Android sessions,
  but the current Mobile App UI connectivity and Search and Book tests are iOS-specific.
  Android is not yet a validated newcomer execution path.

The recommended path for a Windows-based QA is therefore Windows → BrowserStack → iOS app.
