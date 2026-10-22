# Book Core Regression And Playwright HPP Pilot

- Status: active
- Owner: EAPI Automation
- Created: 2026-09-01
- Scope: `.github/workflows`, `VAA.EAPI.Automation.Book`, shared HPP browser support, Automation execution docs

## Objective

Make the scheduled Book GraphQL suite accurately represent a core regression, close the identified journey-coverage gaps, and prove a low-risk Playwright path for one direct hosted-payment-page journey without removing the existing Selenium path.

## Context

The scheduled workflow currently selects 14 `Book-Smoke` scenarios even though it is used as the recurring regression across core Book journeys. It has no daily coverage for PayPal, multi-city, mixed-carrier, logged-in revenue, seat clear-down, or 3DS paths. The direct HPP step also uses Selenium while the newer UI automation owns Playwright support for an embedded HPP. The pilot must preserve the headers applied by the current direct-HPP browser implementation.

## Acceptance Criteria

- The scheduled workflow and reporting use core-regression terminology and a dedicated Book core-regression trait.
- Representative scenarios cover the agreed missing core journey dimensions while Wamos remains on demand.
- Existing scenario categories remain usable for broader/manual execution.
- One explicitly selected direct-HPP journey can use Playwright behind scenario/config opt-in; all other journeys retain Selenium.
- The Playwright browser sends the same non-secret HPP access headers as the Selenium implementation without logging their values.
- Offline tests protect scenario selection, driver routing, and header mapping where practical.
- Automation execution and pipeline documentation describe the renamed suite and pilot.

## Plan

1. Inventory scenario coverage, scheduled filters, category consumers, and reporting names.
2. Introduce the Book core-regression selector and add representative scenario data for missing journeys.
3. Trace the existing Selenium header injection and embedded Playwright HPP implementation.
4. Implement a direct-HPP Playwright adapter and route one scenario to it through an explicit opt-in.
5. Add focused offline tests and update execution/pipeline documentation.
6. Run the Automation harness and targeted validation; record residual live-environment validation needs.

## Decisions

- 2026-09-01: Keep Wamos-operated-leg scenarios in `Book-Functional` for on-demand execution.
- 2026-09-01: Preserve Selenium as the default HPP mechanism until the direct-URL Playwright pilot is proven in SIT/UAT.
- 2026-09-01: Add representative core coverage rather than scheduling all 88 E2E data rows.

## Validation

- `scripts/verify.ps1`
- Targeted offline tests for Book scenario selection and HPP routing/header behaviour
- YAML and JSON/schema validation for changed workflow and scenario data
- Live direct-HPP pilot remains a post-merge environment validation if required secrets or network routes are unavailable locally

## Outcome

- The scheduled Book suite now selects 21 `Book-Core-Regression` scenarios.
- Nine representative scenarios use the direct-HPP Playwright adapter, with Selenium retained as the per-scenario fallback.
- Standard card, PayPal and 3DS payment paths have been proven through live HPP runs.
- External AIR4 post-order seat validation has been removed while Book-owned validation remains.
- Co-brand/Synchrony Playwright support remains experimental and is not enabled because the external application endpoint returns HTTP 503.
- The final Book build completes with zero errors; JSON/schema and diff validation pass.

The implementation and rollout guidance are documented in `VAA.EAPI.Automation/docs/BOOK_CORE_REGRESSION_PLAYWRIGHT_HPP_MIGRATION.md`.
