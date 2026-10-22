# VAA.EAPI.Automation.InFlightApplications.UI

Playwright coverage for the Starlink portal and captive portal customer journeys, migrated from
`VAAD1ST/in-flight-applications/packages/e2e-testing`.

## Architecture

Tests express scenarios, flows orchestrate journeys, pages own every Playwright call and selector,
and assertion helpers validate business outcomes. Do not import Microsoft.Playwright from Tests
or Flows.

## Setup

Copy `local.settings.local.example.json` to `local.settings.local.json` in this project and populate
the values. The local file is ignored by Git and loaded automatically when the test assembly starts.
Existing process environment variables take precedence, so the same tests continue to work with CI
secrets and explicit shell configuration.

The Key Vault fields and `env` use the same names and configuration shape as the other EAPI test
projects. You can copy those values from an existing local settings file; do not copy project-specific
values or commit the populated file. `KeyVaultTenantId`, `KeyVaultClientId` and
`KeyVaultClientSecret` may remain empty when your local Azure identity already has vault access.

```pwsh
Scripts/Setup.ps1
Copy-Item local.settings.local.example.json local.settings.local.json
Scripts/Run-Regression.ps1
```

You can still override the configured URL for one run with
`Scripts/Run-Regression.ps1 -BaseUrl https://replace-with-environment.example`.

The setup script installs Chromium only. The shared Playwright session also supports BrowserStack
through the existing EAPI `SAB_*` settings.

## QA and Codex handover

A QA can ask Codex: **"Run the In-flight automation against the environment configured in the
project's local settings and summarise the TRX results."** Repository guidance maps that request to
the safe non-stateful regression runner. Codex should verify the project-local settings and browser
setup, run `Scripts/Run-Regression.ps1`, and report the results from the new timestamped directory
under `TestResults`.

"Run the In-flight automation" does not include stateful tests by default. To run everything, make
the authority explicit: **"Run all In-flight automation packs, including OTP enrolment and shared-
account journeys, sequentially."** This runs the following in order:

```pwsh
Scripts/Run-Regression.ps1
Scripts/Run-OtpJourneys.ps1
Scripts/Run-AccountJourneys.ps1
```

The OTP pack creates customer enrolments and the account pack mutates configured shared-account
state. They require an approved environment and working EAPI Key Vault/Table Storage access; the
account pack additionally requires all three `E2E_LOGIN_*` values. Never run these two packs in
parallel. Their runners disable Playwright tracing so credentials and OTPs are not captured.

## Test selection

| Trait | Coverage |
| --- | --- |
| `Category=InFlightApplications-UI-Smoke` | Starlink portal and captive portal entry points |
| `Category=InFlightApplications-UI-Validation` | Login, password-reset entry, registration-profile, and registration-email validation |
| `Category=InFlightApplications-UI-Journey` | Non-destructive customer journeys, currently under-18 registration |
| `Category=InFlightApplications-UI-Accessibility` | Reusable axe-core WCAG scans of directly reachable portal pages |
| `Category=InFlightApplications-UI-OtpJourney` | Opt-in adult registration, validation and guarded accessibility coverage |
| `Category=InFlightApplications-UI-AccountJourney` | Opt-in MFA/non-MFA login and password-reset journeys using configured accounts |

Tests skip with a clear message when the base URL is not configured. No test targets production by
default; the caller must supply an approved non-production deployment.

Accessibility scans use `PlaywrightAccessibilityScanner` from `Core.Playwright`. Other Playwright
UI projects can call the same scanner from their page layer and apply suite-specific assertion or
reporting thresholds. The In-Flight suite fails only serious or critical axe violations.

OTP tests are deliberately excluded from the normal regression command. They reuse
the shared `Core.Helpers.OtpStorageCodeReader`, which also backs MyAccount's existing
`EnrolmentHelper` overload, including the existing Key Vault and Azure Table Storage configuration.
Run them only in an approved environment:

```pwsh
Scripts/Run-OtpJourneys.ps1
```

This sets `IN_FLIGHT_APPLICATIONS_RUN_OTP_TESTS=true`. The normal EAPI environment and Key Vault
access required by the EAPI OTP reader must also be available to the process.

Fixed-account journeys use the existing `E2E_LOGIN_EMAIL_MFA`, `E2E_LOGIN_EMAIL_NO_MFA` and
`E2E_LOGIN_PASSWORD` settings. They snapshot the existing OTP row before triggering a new code so
the shared reader cannot return a stale OTP. Run these separately to avoid concurrent mutation of
shared account state:

```pwsh
Scripts/Run-AccountJourneys.ps1
```

## Migration status

The project represents all 45 source feature scenarios as 55 independently reported Playwright
cases; scenario-outline examples account for the different totals. This includes complete adult
Flying Club enrolment through the connection page, address/details validation, preferences,
password validation, MFA/non-MFA login, password-reset OTP validation and all ten source
accessibility pages.
See `MIGRATION_COVERAGE.md` for the feature-to-test reconciliation.

Unlike the source suite, the adult registration journey submits the final password form and verifies
that the newly enrolled Flying Club member reaches the connection page.

The remaining activity is environment verification: run the normal, OTP and fixed-account packs
against an approved deployment and stabilise any environment-specific selectors, data or service
timing before enabling CI execution.

OTP-backed work must reuse EAPI's secure configuration and storage helpers. Credentials and OTPs
must not appear in test names, screenshots, traces, or failure output.
