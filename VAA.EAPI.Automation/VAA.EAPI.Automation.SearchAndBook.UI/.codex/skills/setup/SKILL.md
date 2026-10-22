---
name: setup
description: Set up and verify the VAA Search & Book UI Playwright automation project on macOS or Windows. Use when the user invokes $setup or asks to set up, bootstrap, or prove this automation workspace is ready.
---

# Search & Book UI Setup

Set up the current `VAA.EAPI.Automation.SearchAndBook.UI` workspace and finish only after its offline Playwright verification test passes or a concrete prerequisite blocks it.

## Workflow

1. Confirm the working directory contains `VAA.EAPI.Automation.SearchAndBook.UI.csproj`.
2. Detect the operating system and inspect prerequisites without changing the machine:
   - .NET SDK 8 or later.
   - PowerShell 7 (`pwsh`).
3. If a prerequisite is missing, explain the exact supported installation command and request approval before using a package manager or making a system-wide change. Do not install tools silently.
4. Run the platform entry point from the project directory:

   - macOS: `./Scripts/setup.sh`
   - Windows: `pwsh -NoProfile -File .\Scripts\Setup-SearchAndBookUi.ps1`

5. The setup command must restore and build the project, install the Chromium runtime, run `Category=SearchAndBook-UI-Setup`, and analyse the exact generated TRX.
6. Report the test/analyser result and the paths to the TRX, screenshot, and trace. Do not claim setup succeeded when the verification test was skipped or failed.

The verification is intentionally offline and must not require `SAB_BASE_URL`, credentials, BrowserStack, VPN access, or a live Virgin Atlantic environment. Run the live smoke suite only when the user separately requests it or supplies a target URL.
