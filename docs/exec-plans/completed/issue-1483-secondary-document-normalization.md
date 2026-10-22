# Issue 1483 Secondary Document Normalization

- Status: completed
- Owner: Codex
- Created: 2026-08-21
- Completed: 2026-08-21
- Scope: Ground Operations Passenger Check-In microservice, PaxOps GraphQL domain, and their focused tests/Bruno examples

## Objective

Implement GitHub issue 1483 acceptance criteria 1-8 and QA acceptance criteria 4.3.1-4.3.5 so native clients receive normalized secondary-document names, country labels, application options, and supported guidance.

## Context

- Source: https://github.com/VAAD1ST/eapi/issues/1483, fetched directly from GitHub on 2026-08-21 and re-fetched after implementation.
- Explicitly excluded by request: acceptance criteria 9-11 and QA acceptance criteria 4.3.6-4.3.8.
- The response spans the Check-In microservice contract and PaxOps GraphQL reference-data enrichment.

## Acceptance Criteria

- Resolve customer-facing country names through the Reference API and derive display names from that country name plus the raw TRM visa name, with the US ETA/ESTA exception.
- Return exactly one VENDOR and one GOVERNMENT option per applicable requirement, duplicating the source ID when one option is generated.
- Use backend-owned Sherpa/ESTA links and guidance, return a null government URL when no approved URL is defined, and filter visa-on-arrival requirements.
- Cover vendor-only, dual-source, electronic visa, ESTA, visa-on-arrival, and multi-document scenarios with focused tests.
- Keep GraphQL examples aligned with the changed response contract.

## Plan

1. Trace the existing TRM-to-Check-In and Check-In-to-GraphQL mappings.
2. Implement normalization at the owning mapper and GraphQL enrichment boundaries.
3. Update contract models, mocks, Bruno selections, and focused tests.
4. Run project-scoped test suites with TRX, coverage, and analyser output.
5. Re-fetch the GitHub ticket, review scope, and record the outcome.

## Decisions

- 2026-08-21: Preserve existing blocked/error status behavior because AC 9-11 and QA AC 4.3.6-4.3.8 are excluded.
- 2026-08-21: Do not return arbitrary TRM URLs. The configured Sherpa link and ticket-defined ESTA URL are backend-owned; generic government URLs remain null.
- 2026-08-21: Determine ESTA exclusively in Check-InMS when the first TRM `requirementOptions` value is `ETA` and `country.isoAlpha3Code` is exactly `USA`; GraphQL does not re-detect or rewrite ESTA application content.
- 2026-08-21: Check-InMS cannot call another microservice. It emits `{countryName}` placeholders for non-ESTA display names and government guidance; GraphQL resolves only those placeholders using ReferenceMS country data.
- 2026-08-21: Do not search TRM links for a `vendor` source. Check-InMS always creates the VENDOR option with the configured Sherpa deep link and inspects other links without source-specific vendor handling only for identity/name preservation.
- 2026-08-21: Remove fallback-link handling. Each present option resolves its own ID; when one source option is absent, the generated option explicitly copies the resolved ID from the present option.
- 2026-08-21: Replace ESTA-specific content branching with a country-and-requirement-option override table. The USA/ETA entry supplies the display name, government title, and URL; unmatched combinations use generic defaults.
- 2026-08-24: Remove `bulletPoints` from the Check-InMS and GraphQL contracts, tests, mocks, and Bruno selections. ESTA now uses the generic government guidance message rather than a custom override.
- 2026-08-24: Remove `subtitle` from the Check-InMS and GraphQL contracts, mappings, tests, and Bruno selections.
- 2026-08-24: Move the country-and-requirement content override table into the `SecondaryDocumentContentOverrides` CheckInMS configuration variable and wire it through all deployment environments.

## Validation

- Passenger Check-In focused tests: 44 passed, 0 failed; analyser found no failures; 10.45% line and 11.17% branch coverage.
- PaxOps GraphQL focused tests: 16 passed, 0 failed; analyser found no failures; 10.54% line and 7.40% branch coverage.
- Passenger Check-In full project tests: 98 passed, 0 failed; analyser found no failures; 16.87% line and 19.12% branch coverage.
- PaxOps GraphQL full project tests: 133 passed, 0 failed; analyser found no failures; 28.59% line and 22.87% branch coverage.
- Post-review Passenger Check-In focused tests: 45 passed, 0 failed; analyser found no failures; 10.47% line and 11.21% branch coverage.
- Post-review PaxOps GraphQL focused tests: 16 passed, 0 failed; analyser found no failures; 10.36% line and 7.18% branch coverage.
- Post-review Passenger Check-In full project tests: 99 passed, 0 failed; analyser found no failures; 16.89% line and 19.14% branch coverage.
- Post-review PaxOps GraphQL full project tests: 133 passed, 0 failed; analyser found no failures; 28.44% line and 22.69% branch coverage.
- ASCII guidance verification: Passenger Check-In 45 passed and PaxOps GraphQL 16 passed; both analysers found no failures.
- Final service-boundary validation: Passenger Check-In full project 99 passed with 16.87% line/19.09% branch coverage; PaxOps GraphQL full project 133 passed with 28.47% line/22.76% branch coverage; both analysers found no failures.
- Sherpa-link refinement: Passenger Check-In focused project 45 passed with 10.46% line/11.12% branch coverage; the analyser found no failures.
- Fallback-link removal: Passenger Check-In focused project 45 passed with 10.47% line/11.12% branch coverage; the analyser found no failures.
- Content-override refactor: Passenger Check-In focused project 45 passed with 10.55% line/11.21% branch coverage; the analyser found no failures.
- Bullet-point and ESTA-message removal: Passenger Check-In focused project 45 passed with 13.21% line/9.54% branch coverage; PaxOps GraphQL focused project 16 passed with 10.38% line/7.28% branch coverage; both analysers found no failures.
- Final full-project validation after removal: Passenger Check-In 99 passed with 16.89% line/19.07% branch coverage; PaxOps GraphQL 133 passed with 28.46% line/22.76% branch coverage; both analysers found no failures.
- Subtitle removal: Passenger Check-In full project 99 passed with 16.88% line/19.04% branch coverage; PaxOps GraphQL full project 133 passed with 28.45% line/22.76% branch coverage; both analysers found no failures.
- Configured content overrides: Passenger Check-In focused project 45 passed with 10.47% line/11.17% branch coverage; full project 99 passed with 16.87% line/19.08% branch coverage; both analysers found no failures. All 13 CheckIn deployment JSON files parsed successfully.
- `git diff --check` completed without whitespace errors; Git reported only line-ending conversion notices.
- The live issue and comments were re-fetched after implementation; the issue remained last updated at `2026-08-20T15:37:15Z`.

## Outcome

The Check-In mapper now normalizes applicable secondary-document requirements into backend-owned VENDOR/GOVERNMENT option pairs, filters visa-on-arrival items, preserves raw names, and owns the US ETA-to-ESTA exception and application guidance. For non-ESTA requirements it emits country-name placeholders rather than using TRM country names. PaxOps obtains the customer-facing country name from ReferenceMS and substitutes those placeholders without performing ESTA detection or rewriting ESTA content. Excluded blocked, unavailable, dependency-failure, false-completion, and dynamic-recovery behavior was not changed.
