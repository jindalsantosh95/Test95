# Search and Book UI Automation Rationale

## Decision

Search and Book UI automation should live in `VAA.EAPI.Automation` alongside the
existing Search, Book and shared automation capabilities. The UI layer uses
Playwright in C# and extends the existing harness rather than creating another
standalone repository with a separate implementation of the same journey.

## Why EAPI is the right home

### One journey, one source of truth

An end-to-end booking starts with search and continues through availability,
offer selection, passenger details, seats, payment and confirmation. The same
business journey also depends on backend services, test data and diagnostics
already represented in EAPI. Keeping UI coverage here avoids each repository
creating its own search setup, basket handling, account lookup and booking
confirmation logic.

### Reuse the established harness

EAPI already provides common test execution conventions, environment handling,
test data access, backend API coverage and CI integration. The Playwright
projects add browser interaction while preserving those conventions. The result
is a consistent test shape for API, UI and mobile coverage rather than a new
tool-specific silo.

### Better failure diagnosis

The same test run can correlate UI evidence with service behaviour:

- Playwright traces and screenshots show the browser state and timing.
- `network-diagnostics.json` records failed requests and HTTP 4xx/5xx responses.
- Existing EAPI API clients and test utilities can investigate basket, booking
  and confirmation failures without reproducing setup in a separate repository.

This makes it possible to distinguish a product/API failure from a UI locator or
timing failure quickly. For example, the Search and Book implementation reached payment and
identified an STG confirmation API `422` response rather than reporting a vague
confirmation-page timeout.

### Lower maintenance cost

Separate UI repositories tend to duplicate the following over time:

- browser and device configuration
- secrets and CI workflow logic
- test accounts, loyalty data and booking data setup
- page-object and journey utilities
- Slack/reporting conventions and failure diagnostics

Centralising the harness reduces duplicate fixes and makes framework upgrades,
security updates and guardrails apply consistently across automation types.

### Clear technology boundary without fragmented ownership

Playwright is appropriate for the website UI layer. It does not require a
separate automation estate: it is a focused UI adapter within the existing C#
automation solution. BrowserStack integration can use the same environment,
reporting and execution conventions as other EAPI test suites while retaining
Playwright's browser and device capabilities.

## Benefits for Harness Engineering

This structure supports future AI-assisted test creation without sacrificing
governance. A scenario expressed in business language can be implemented using
shared page objects, data providers, assertions and reporting rather than
generating independent frameworks per feature. Guardrails, review standards and
cross-browser execution stay central while the scenario coverage grows.

## Scope of the Current Implementation

The implementation establishes the following foundation:

- C# Playwright shared browser/session harness.
- Local headed/headless and BrowserStack execution modes.
- Search-to-payment journey page objects and staged smoke, deep and E2E tests.
- Right-side price-summary checks across the journey.
- Confirmation checks for booking reference, passenger, flight and payment data.
- Trace, screenshot and network-failure artifacts.

The next increments should expand scenario coverage and CI/device-matrix
execution on this shared foundation rather than duplicating it in a separate UI
repository.
