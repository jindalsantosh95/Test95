# CI Pipeline Map (VAA.EAPI.Automation)

This document maps Automation projects to the repository-owned GitHub Actions and
Azure DevOps execution patterns. It was reconciled with the checked-in workflows on
2026-09-11.

## Pipeline Model (High-Level)

Hosted metadata jobs, Automation build/harness and GraphQL Search/MyAccount jobs
use `ubuntu-latest`. C# checkout remains CRLF through `.gitattributes`, preserving
the existing formatting gate. Self-hosted Windows suites retain their runner labels.
`linux-runner-validation.yml` exercises offline build/tests, portable resource paths,
manifest routing and scheduled harness exercises without live environment secrets.
Offline harness tests retain run-scoped TRX files in `TestResults`; analyser summaries
are printed in the job log.

- The dedicated `verify` status check runs the Automation harness for relevant changes
  and reports success for unrelated changes so branch protection never waits on a
  skipped required check.
- A separate path-filtered workflow restores and builds the full Automation solution
  and publishes its Release binaries.
- Environment-dependent smoke and regression suites run through explicit GitHub
  workflows or Azure DevOps functional-test templates with authorised settings,
  secrets, data and runners.
- Shared regression mechanics live in reusable GitHub workflows. Suite manifests own project
  paths, filters, runner selection, timeout, reporting configuration, ownership and required
  setting names. Thin suite workflows own schedules, manual inputs and secret mapping.

## Repository-Owned Workflow Map

| Scope | Definition | Trigger | Purpose / filter source | Enforcement |
|---|---|---|---|---|
| Automation harness | `.github/workflows/automation-harness.yml` | Every PR and push to `main`; internal path detection | Runs `VAA.EAPI.Automation/scripts/verify.ps1`, including the warning ratchet, offline architecture/Core tests and new-file formatting. | Required `verify` status check on `main` |
| Automation build | `.github/workflows/vaa-eapi-automation-ci.yml` | PR and push to `main` when `VAA.EAPI.Automation/**` changes | Restores/builds `VAA.EAPI.Automation.sln` in Release and uploads binaries. | CI build evidence; not the stable always-reporting branch-protection check |
| Search and Book core regression | `.github/workflows/search-book-smoke.yml` | Manual dispatch and workflow-defined schedules | Runs the scheduled core Search and Book journeys against selected environments, with an exact HPP page-origin allowlist for Playwright payment access. The filename is retained so GitHub Actions history and links remain stable. | Environment-dependent operational sensor |
| Book GraphQL ad-hoc journey | `.github/workflows/book-graphql-ad-hoc.yml` | Manual dispatch | Runs a data-driven Book journey with the selected HPP mode and the same exact Playwright HPP page-origin boundary. | Manual functional evidence |
| Search and Book UI BrowserStack regression | `.github/workflows/search-book-ui-browserstack-smoke.yml` | Daily schedule and manual dispatch | Runs one Playwright UI scenario per BrowserStack shard. Each remote session captures its egress IP, verifies frontend access and uploads sanitised preflight/network evidence. | Environment-dependent cross-browser regression sensor |
| My Account GraphQL smoke | `.github/workflows/myaccount-graphql-smoke.yml` | Manual dispatch and workflow-defined schedules | Runs the MyAccount project with its workflow-owned category filter and retained result evidence. | Environment-dependent operational sensor |
| Starlink daily UI regression | `.github/workflows/starlink-daily-regression.yml` + `config/automation-suites/starlink-daily-regression.json` | Daily at 15:00 UTC and manual dispatch; always targets `test3` | Manifest-driven run of the In-Flight Applications UI Smoke, Validation, Journey and Accessibility categories on the `self-hosted`, `Windows`, `eapi-automation` runner through BrowserStack. The reusable runner reads `IN_FLIGHT_APPLICATIONS_UI_BASE_URL` from the `test3` environment and uses the repository/org secret `SLACK_WEBHOOK_URL_STARLINK`, validates each category's TRX/test count, publishes the summary, uploads run/attempt-scoped evidence and posts to Slack. | Scheduled/manual operational sensor |
| Reusable regression runner | `.github/workflows/qa-reusable-regression-build-test.yml` | `workflow_call` only | Central build/test, evidence, summary, HPP page-origin allowlist and notification implementation used by suite workflows. | Inherited by its callers |
| Manifest-driven suite wrapper | `.github/workflows/qa-run-automation-suite.yml` | `workflow_call` only | Validates a suite manifest, maps standard repository variables and the exact HPP page origin, then delegates to the reusable regression runner. | Preferred onboarding contract for new API/GraphQL automation suites |
| Trips backend regression | `.github/workflows/qa-trips-backend-regression.yml` | Manual dispatch plus weekday schedules | Calls the reusable runner for `VAA.EAPI.Automation.Trips` with checked-in Trips regression categories and summary configuration. | Scheduled/manual functional evidence |
| Trips PNR creation | `.github/workflows/qa-trips-pnr-creation.yml` | Manual dispatch plus daily schedules | Calls the reusable runner for `VAA.EAPI.Automation.Trips` with PNR-creation categories and summary configuration. | Scheduled/manual functional evidence |
| Shared-services sign-off | `.github/workflows/shared-services-sign-off.yml` | Manual dispatch with environment, shared-service, consumer and HPP-mode inputs | Runs the selected Book, Search, MyAccount, Trips, FlightOperations, OCI and MarketingLoyalty projects using `Category=SharedServicesSignOff` plus optional `SharedService` and `SharedServiceConsumer` filters on the `self-hosted`, `Windows`, `eapi-automation` runner. Analyses each run-scoped TRX, uploads evidence and posts one aggregate result to `shared-services-signoff-results`. | Manual shared-service consumer evidence; fails when a selected project produces no passing test evidence |

`VAA.EAPI.Automation.Utility.DataProvisioning` is not yet connected to a repository workflow. The Trips row above describes the legacy xUnit data-creation path. The proposed Playwright manifest workflow, safe rollout order and required reporting/storage contract are maintained in [Data provisioning Playwright pilot](DATA_PROVISIONING_PLAYWRIGHT_PILOT.md#scheduled-workflow-rollout); add the new workflow rows here when those definitions are introduced.

### Starlink Daily UI Regression

The Starlink entry above is owned by the In-Flight Applications suite manifest and the thin
`.github/workflows/starlink-daily-regression.yml` schedule/dispatch wrapper. The wrapper delegates
execution to `qa-run-automation-suite.yml`, which resolves the manifest before invoking the shared
Windows runner. The workflow always uses the `test3` GitHub Environment for the base URL and passes
the repository/org secret `SLACK_WEBHOOK_URL_STARLINK` to the reusable runner, matching the existing
suite secret pattern. BrowserStack credentials remain explicitly mapped from the existing secrets.
The runner retains category TRX files and BrowserStack network diagnostics in the run/attempt-scoped
artifact and sends the configured summary to Slack.

## Azure DevOps Integration

### Native mobile Sanity (manual, SIT only)

`qa-mobile-search-book-sanity.yml` uses the same `[self-hosted, Windows, eapi-automation]`
labels as Book ad-hoc automation, using Windows PowerShell (`powershell`). The job installs
.NET SDK `10.0.x` into the runner's temporary directory, without writing to Program Files
or requiring `pwsh` on that runner. Hosted Linux metadata checks continue to use `pwsh`.
Its scaffolded manifest is
`config/automation-suites/mobile-search-book-sanity.json`. This specialised mobile workflow
consumes the explicit `testFilter` (which overrides category metadata); do not send it through
the legacy category-only reusable runner. Existing category-based manifests are unchanged.

The workflow display name is **Automation - Mobile Search and Book**. The `pack` input
selects Sanity (14, default) or Regression (54 including backend/UI comparisons TC41/42 and card-presentation TC55).
Regression enables SIT backend
validation on the self-hosted runner. Leave `test_name` blank for packs; an exact test
requires the default Sanity input and is reported as Targeted. Dispatch from the PR
branch to test unmerged automation. Exact TRX membership must match the selected pack.
It runs four BrowserStack sessions at most,
with project `Virgin Atlantic Mobile - Search and Book`. App ID is a dispatch input and the
build name includes run ID/attempt. Credentials use existing `BROWSERSTACK_USERNAME`,
`BROWSERSTACK_ACCESS_KEY`, `LOYALTY_ACCOUNT_PASSWORD`, the four `KEY_VAULT_*` secrets and
`SLACK_WEBHOOK_URL`. The run writes temporary project/output settings and removes them in
an always-run cleanup step. The private `ios-coverage-source` checkout is deleted in an
always-run step immediately after source reporting. If the runner is forcibly lost, operators
must remove both generated settings files and that private checkout from the job workspace
before reusing the runner.

Optional manual input `test_name` accepts the exact method name from the iOS UI Sanity
or Regression release pack, for example
`SearchBook_TC48_OneAdultGuestWithoutSeats_CompletesBookingAndAppearsInTrips`.
Leave it blank for the complete Sanity pack. Do not enter a TC number, class name,
substring or VSTest filter. The resolver rejects unknown, deferred and backend-comparison
scenarios before allocating the private runner. A targeted run uses an exact fully qualified
filter plus iOS/SearchAndBook traits, requires precisely that one passing result, and is
labelled `Targeted` in the BrowserStack build and Slack summary; it is not Sanity sign-off.
Full Sanity validates scenario identities as well as count and outcomes. Runner-side offline
fixtures check reporting and selection under Windows PowerShell before accessing credentials.
Settings are never uploaded. Result/diagnostic artifacts have seven-day retention.

Always-run result upload and Slack steps preserve test failures and report confirmed test
booking references. Missing/skipped/short runs cannot pass the job. This first workflow does
not run backend comparisons or the five-passenger TC01. It creates test bookings and consumes
test loyalty points. No schedule or PR trigger is enabled. Initial live qualification is pending;
the workflow must be present on the default branch before its first manual dispatch.

Azure DevOps definitions remain under `VAA.EAPI.Pipelines/` and
`VAA.EAPI.Pipelines.DigitalServices/`. The principal Automation entry points are:

- `VAA.EAPI.Pipelines/EAPIAutomationTesting.AFN.CI.Pipeline.yaml` and
  `EAPIAutomationTesting.PR.AFN.CI.Pipeline.yaml` for Automation CI/PR integration;
- `VAA.EAPI.Pipelines/functions/functional_testing*.yaml` and domain subfolders for
  post-deployment functional suites;
- `VAA.EAPI.Pipelines.DigitalServices/functions_digital/functional_testing*.yaml` and
  domain subfolders for Digital Services suites;
- `VAA.EAPI.Pipelines/TestRunner.SVC.CICD.Pipeline.yaml` publishes the TestRunner service and
  its referenced Heimdall assemblies. Its path filters include `VAA.EAPI.Heimdall`,
  `VAA.EAPI.Automation.Heimdall.UI`, and the shared Search & Book/Core Playwright UI projects so
  monitor or framework changes rebuild the deployed synthetic-runner artifact;
- build definitions that publish the `testAutomation` artifact consumed by downstream
  functional-test jobs.

These files are deployment and environment execution wiring. The GitHub `verify` job
remains the canonical harness enforcement entry in
`docs/harness-pipeline-checklist.md`.

## Required Pipeline Inputs

For each new or changed suite, create `config/automation-suites/<slug>.json` using
`scripts/new-automation-suite.ps1`. The manifest records:

- owning team, project and executable categories;
- runner labels and timeout;
- required repository variables and secret names;
- optional allowlist of non-secret test-process variable names;
- optional summary and Slack configuration.

The thin owning workflow records the schedule, manual target-environment choices and maps standard
secret names. Environment-dependent values remain in GitHub variables and secrets, never in the
manifest. Bespoke UI matrices or multi-stage provisioning may retain dedicated workflows, but
should still reuse common build, result and cleanup scripts where their execution model permits.

## Maintenance Rules

- Update this file whenever a pipeline path, trigger, reusable workflow or ownership
  model changes.
- Use the repo-local `new-automation-suite` skill for ordinary API/GraphQL suite onboarding.
- Prefix user-facing Automation workflow names with `Automation -` so they remain grouped in the
  GitHub Actions navigation. Do not rename stable job/check identifiers solely for presentation.
- Keep owner team names current for faster triage.
- Keep detailed category filters in suite manifests rather than duplicating them in workflow YAML
  or prose documentation.
- Preserve `automation-harness.yml` as an always-triggering workflow with internal
  no-op logic while `verify` is a required status check.

## Mobile source-aware reporting

External-model coverage assessment is deferred until a dedicated restricted runner or equivalent
network boundary is approved and validated. No mobile AI workflow or model call is included.
The existing Book runner remains for functional tests, not AI processing.

The native Mobile Search and Book Sanity workflow inventories iOS SearchAndBook source at
`ios_ref` and attaches a current-run mapping report to its artifacts/Slack summary. It reuses
the sequence-goal GitHub App credentials with contents-read access limited to `vaa-mobile-ios`.
Missing access reports coverage unavailable without substituting an old inventory. Optional
`app_commit` declares build identity; it is not automatic build attestation. See
[`IOS-SOURCE-COVERAGE.md`](../VAA.EAPI.Automation.MobileApp.UI/IOS-SOURCE-COVERAGE.md).
