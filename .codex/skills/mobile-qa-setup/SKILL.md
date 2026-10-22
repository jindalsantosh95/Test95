---
name: mobile-qa-setup
description: Set up and validate the VAA EAPI Mobile App UI Appium/xUnit project. Use when a QA asks Codex to "setup", "set up mobile tests", check BrowserStack connectivity, or verify that the mobile automation project is ready to run. Do not use this workflow to run booking or payment journeys.
---

# Mobile QA Setup

Run the repository's deterministic setup workflow rather than reconstructing Appium or BrowserStack commands manually:

```pwsh
VAA.EAPI.Automation/scripts/setup-mobile-app-ui.ps1
```

The workflow checks the .NET 10 SDK, creates a gitignored local settings file from the safe example when needed, restores and builds the project, runs offline harness tests, validates the selected local-Appium or BrowserStack mode without printing secrets, and runs only `Category=MobileApp-Connectivity` when configuration is complete.

Never substitute an unfiltered `dotnet test` command: that can execute real booking and payment journeys. Do not invent or request credentials in chat. If the workflow reports missing values, tell the QA which properties in the gitignored `local.settings.local.json` are missing and let them configure those values securely before rerunning the setup command.

Use `-SkipConnectivityTest` when the QA explicitly wants offline setup only. Report the setup script's outcome and the generated `TestResults/mobile-setup-*` location. Do not run `MobileApp-SearchBook-SmokeE2E` unless the user explicitly requests the real E2E journey and confirms the appropriate environment is ready.
