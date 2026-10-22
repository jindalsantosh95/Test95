# Harness Pipeline Checklist

This checklist keeps CI controls aligned with local harness controls for any solution or project in the repository.

## Enforcement Policy

- `required`: blocking in PR/main pipelines and branch protection status checks.
- `advisory`: non-blocking, but tracked and reviewed on a schedule.
- Required checks must fail closed. Do not use trigger-level path filters for required workflows; use always-trigger with internal no-op logic when scope is not impacted.

## Enforcement Inventory

Use this table as the operational source of truth for solution/project harness enforcement.

| Scope | Owner Team | Canonical Verify Command | Pipeline Path | Required Status Check | Branch Protection | Enforcement Status | Notes |
|---|---|---|---|---|---|---|---|
| `VAA.EAPI.Automation/VAA.EAPI.Automation.sln` | Automation Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.Automation/VAA.EAPI.Automation.sln -SkipBuild` | `.github/workflows/ai-harness-vaa-eapi-automation.yml` | `verify` | `main` protected with `verify` required | Required | Keep always-trigger + internal skip logic. |
| `VAA.EAPI.Deployments/VAA.EAPI.Deployments.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.Deployments/VAA.EAPI.Deployments.sln -SkipBuild` | `.github/workflows/ai-harness-repository-parity.yml` | `parity-gate` | `main` should require `parity-gate` | Required via repo parity | Keep `parity-gate` as the stable required check until solution-owned gates are introduced. |
| `VAA.EAPI.Facade/VAA.Facade.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.Facade/VAA.Facade.sln -SkipBuild` | `.github/workflows/ai-harness-repository-parity.yml` | `parity-gate` | `main` should require `parity-gate` | Required via repo parity | Keep `parity-gate` as the stable required check until solution-owned gates are introduced. |
| `VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln -SkipBuild` | `.github/workflows/ai-harness-repository-parity.yml` | `parity-gate` | `main` should require `parity-gate` | Required via repo parity | Keep `parity-gate` as the stable required check until solution-owned gates are introduced. |
| `VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Stitch.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Stitch.sln -SkipBuild` | `.github/workflows/ai-harness-repository-parity.yml` | `parity-gate` | `main` should require `parity-gate` | Required via repo parity | Keep `parity-gate` as the stable required check until solution-owned gates are introduced. |
| `VAA.EAPI.Microservices/VAA.EAPI.Microservices.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.Microservices/VAA.EAPI.Microservices.sln -SkipBuild` | `.github/workflows/ai-harness-repository-parity.yml` | `parity-gate` | `main` should require `parity-gate` | Required via repo parity | Keep `parity-gate` as the stable required check until solution-owned gates are introduced. |
| `VAA.EAPI.Microservices/VAA.EAPI.Microservices.CRM.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.Microservices/VAA.EAPI.Microservices.CRM.sln -SkipBuild` | `.github/workflows/ai-harness-repository-parity.yml` | `parity-gate` | `main` should require `parity-gate` | Required via repo parity | Keep `parity-gate` as the stable required check until solution-owned gates are introduced. |
| `VAA.EAPI.Microservices/VAA.EAPI.Microservices.Data.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.Microservices/VAA.EAPI.Microservices.Data.sln -SkipBuild` | `.github/workflows/ai-harness-repository-parity.yml` | `parity-gate` | `main` should require `parity-gate` | Required via repo parity | Keep `parity-gate` as the stable required check until solution-owned gates are introduced. |
| `VAA.EAPI.Microservices/VAA.EAPI.Microservices.FlightOperations.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.Microservices/VAA.EAPI.Microservices.FlightOperations.sln -SkipBuild` | `.github/workflows/ai-harness-repository-parity.yml` | `parity-gate` | `main` should require `parity-gate` | Required via repo parity | Keep `parity-gate` as the stable required check until solution-owned gates are introduced. |
| `VAA.EAPI.Microservices/VAA.EAPI.Microservices.GroundOperations.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.Microservices/VAA.EAPI.Microservices.GroundOperations.sln -SkipBuild` | `.github/workflows/ai-harness-repository-parity.yml` | `parity-gate` | `main` should require `parity-gate` | Required via repo parity | Keep `parity-gate` as the stable required check until solution-owned gates are introduced. |
| `VAA.EAPI.Microservices/VAA.EAPI.Microservices.Loyalty.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.Microservices/VAA.EAPI.Microservices.Loyalty.sln -SkipBuild` | `.github/workflows/ai-harness-repository-parity.yml` | `parity-gate` | `main` should require `parity-gate` | Required via repo parity | Keep `parity-gate` as the stable required check until solution-owned gates are introduced. |
| `VAA.EAPI.Microservices/VAA.EAPI.Microservices.Retail.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.Microservices/VAA.EAPI.Microservices.Retail.sln -SkipBuild` | `.github/workflows/ai-harness-repository-parity.yml` | `parity-gate` | `main` should require `parity-gate` | Required via repo parity | Keep `parity-gate` as the stable required check until solution-owned gates are introduced. |
| `VAA.EAPI.Microservices/VAA.EAPI.Microservices.Retail.Holidays.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.Microservices/VAA.EAPI.Microservices.Retail.Holidays.sln -SkipBuild` | `.github/workflows/ai-harness-repository-parity.yml` | `parity-gate` | `main` should require `parity-gate` | Required via repo parity | Keep `parity-gate` as the stable required check until solution-owned gates are introduced. |
| `VAA.EAPI.Microservices/VAA.EAPI.Microservices.Utilities.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.Microservices/VAA.EAPI.Microservices.Utilities.sln -SkipBuild` | `.github/workflows/ai-harness-repository-parity.yml` | `parity-gate` | `main` should require `parity-gate` | Required via repo parity | Keep `parity-gate` as the stable required check until solution-owned gates are introduced. |
| `VAA.EAPI.NDC.Transformation/VAA.EAPI.NDC.Transformation.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.NDC.Transformation/VAA.EAPI.NDC.Transformation.sln -SkipBuild` | `.github/workflows/ai-harness-repository-parity.yml` | `parity-gate` | `main` should require `parity-gate` | Required via repo parity | Keep `parity-gate` as the stable required check until solution-owned gates are introduced. |
| `VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.sln -SkipBuild` | `.github/workflows/ai-harness-repository-parity.yml` | `parity-gate` | `main` should require `parity-gate` | Required via repo parity | Keep `parity-gate` as the stable required check until solution-owned gates are introduced. |
| `VAA.EAPI.Web/VAA.EAPI.Web.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.Web/VAA.EAPI.Web.sln -SkipBuild` | `.github/workflows/ai-harness-repository-parity.yml` | `parity-gate` | `main` should require `parity-gate` | Required via repo parity | Keep `parity-gate` as the stable required check until solution-owned gates are introduced. |
| `VAA.EAPI.HealthStatusApp/VAA.EAPI.HealthStatusApp.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.HealthStatusApp/VAA.EAPI.HealthStatusApp.sln -SkipBuild` | Not enrolled in required CI | None (inactive scope) | n/a (inactive scope) | Inactive | Retained in repository but not enrolled in required harness enforcement scope. |
| `VAA.EAPI.LoungeManagement/VAA.EAPI.LoungeManagement.sln` | Platform Engineering | `scripts/verify.ps1 -SolutionPath VAA.EAPI.LoungeManagement/VAA.EAPI.LoungeManagement.sln -SkipBuild` | Not enrolled in required CI | None (inactive scope) | n/a (inactive scope) | Inactive | Retained in repository but not enrolled in required harness enforcement scope. |

Maintenance rule:
Refresh this inventory whenever ownership, verify commands, pipeline paths, or required check names change.

Status rule:
- `Required` and `Required via repo parity` are active enforcement rows.
- `Inactive` rows are intentionally out of required CI enforcement and must include a reason in Notes.

## Branch Protection Required Checks (main)

Configure `main` branch protection to require these stable checks:

- `parity-gate` from `.github/workflows/ai-harness-repository-parity.yml`
- `harness-coverage` from `.github/workflows/ai-harness-high-risk-coverage.yml`
- `verify` from `.github/workflows/ai-harness-vaa-eapi-automation.yml` (while automation scope keeps this dedicated gate)
- `closed-loop-gate` from `.github/workflows/ai-harness-closed-loop-autonomy.yml`

Record branch-protection confirmation and drift remediation outcomes in [`harness-remediation-log.md`](harness-remediation-log.md).
Include explicit bypass-policy declaration per required check and maintain at least 2 passing drift-review cycles for closed-loop evidence.

## Purpose

- Re-run fast local sensors in CI for parity.
- Add heavier post-integration sensors in CI.
- Make pass/fail policy explicit for solution/project changes.

## Pull Request Pipeline

Run restore/build/test commands from the relevant solution/project folder, or pass explicit paths from the repository root.

- [ ] Restore (`required`): `dotnet restore <solution>.sln` or `dotnet restore <project>.csproj`.
- [ ] Build (`required`): `dotnet build <solution>.sln -c Release --nologo` or `dotnet build <project>.csproj -c Release --nologo`.
- [ ] Tests (`required`): `dotnet test <test-project>.csproj -c Release --no-build --nologo` for impacted test projects.
- [ ] Behavior contract coverage (`required` for high-risk behavior changes): verify impacted sequence diagrams, workflows, routes, events, or payloads are listed in `docs/behavior-harness.md` or project docs with tests or an explicit proposed status.
- [ ] Harness coverage check (`required` for high-risk changes): verify the relevant failure modes have guides and sensors listed in `docs/harness-coverage.md`, project docs, or PR notes.
- [ ] Knowledge health check (`required`) from repo root: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/check-knowledge-health.ps1`.
- [ ] Harness rule lint (`required`) from repo root: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/check-harness-rules.ps1`.
- [ ] Agent-memory contract lint (`required`) from repo root: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/check-agent-memory-state.ps1`. CI validates the tracked example; a local state file is validated only when present.
- [ ] Context-routing lint (`required`) from repo root: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/check-context-routing.ps1 -MaxAgeDays 30`.
- [ ] Operational harness registry lint (`required`) from repo root: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/check-operational-harness.ps1`.
- [ ] Agent resource-budget exercise (`required`) from repo root: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/invoke-agent-resource-budget-exercise.ps1 -EvidenceDirectory TestResults/agent-resource-budget/pipeline`; validate with `scripts/check-agent-resource-budget-strength.ps1`.
- [ ] Tool/environment probe and mutation exercise (`required`): `scripts/check-agent-tool-environment.ps1 -Profile docs-local -Probe`, then `scripts/invoke-agent-tool-environment-exercise.ps1`.
- [ ] Bounded observability query exercise (`required`): `scripts/invoke-agent-observability-exercise.ps1`.
- [ ] Continuous-maturity transition exercise (`required`): `scripts/invoke-harness-maturity-evidence-exercise.ps1`.
- [ ] Harness parity run (`required`) from repo root: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/verify.ps1 -SolutionPath <solution>.sln -SkipBuild` or `-ProjectPath <project>.csproj -SkipBuild`.
- [ ] Deployment project check (`required` when Azure Functions projects are impacted) from repo root: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/check-deployment-projects.ps1`.
- [ ] Fail build if any required step fails (`required`).

### High-Risk Label Usage

Use the `high-risk` label on pull requests that touch security, identity/auth, payment, PII, shared startup/runtime wiring, critical integration contracts, or customer-impacting workflow behavior.

When `high-risk` is present:

- `ai-harness-high-risk-coverage` workflow is required.
- `ai-harness-high-risk-autolabel` workflow should automatically apply the label when high-risk files are changed.
- PR description must include `## Harness Coverage` with non-empty values for:
  - `Failure mode`
  - `Guide`
  - `Sensor`
  - `Lifecycle`
  - `Policy`
- PR description must include `## Context Routing` with non-empty values for:
  - `Scope`
  - `Dependencies`
  - `Assumptions`
  - `Impacted sensors`
  - `Rollback/owner`
- Any accepted gap must name owner and reopen condition.
- [ ] Restore: `dotnet restore <solution>.sln` or `dotnet restore <project>.csproj`
- [ ] Build: `dotnet build <solution>.sln -c Release --nologo` or `dotnet build <project>.csproj -c Release --nologo`
- [ ] Tests: `dotnet test <test-project>.csproj -c Release --no-build --nologo` for impacted test projects.
- [ ] Behavior contract coverage: verify impacted sequence diagrams, workflows, routes, events, or payloads are listed in `docs/behavior-harness.md` or project docs with tests or an explicit proposed status.
- [ ] Harness coverage check: for high-risk changes, verify the relevant failure modes have guides and sensors listed in `docs/harness-coverage.md`, project docs, or the PR notes.
- [ ] Knowledge health check from repo root: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/check-knowledge-health.ps1`.
- [ ] Harness rule lint from repo root: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/check-harness-rules.ps1`.
- [ ] Sequence diagram status check from repo root: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/check-sequence-diagram-status.ps1`.
- [ ] Harness parity run from repo root: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/verify.ps1 -SolutionPath <solution>.sln -SkipBuild` or `-ProjectPath <project>.csproj -SkipBuild`.
- [ ] Deployment project check from repo root when Azure Functions projects are impacted: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/check-deployment-projects.ps1`.
- [ ] Fail build if any required step fails.

## Main Branch Pipeline

- [ ] Repeat all pull request required steps (`required`).
- [ ] Full harness run (`required`): `scripts/verify.ps1 -SolutionPath <solution>.sln` or `scripts/verify.ps1 -ProjectPath <project>.csproj`.
- [ ] Collect and publish test result artifacts (`required`).
- [ ] Record duration metrics for restore/build/test and track trend (`advisory`).
- [ ] For autonomous mutating workflows, publish the execution record, checkpoint, timeout/attempt budget, and sanitized terminal evidence defined in [`agent-execution-and-recovery.md`](agent-execution-and-recovery.md) (`required` when enrolled).
- [ ] For every enrolled Codex workflow, retain JSONL usage and the budget decision required by [`agent-resource-and-cost-controls.md`](agent-resource-and-cost-controls.md); fail before accepting output if the budget check fails (`required`).

## Scheduled Drift Pipeline

- [ ] Execute full harness run for selected high-value solutions or projects (`required` for enrolled scopes).
- [ ] Export trend metrics: failures by step, duration by step, flaky test count (`advisory`).
- [ ] Run doc-gardening drift scan from repo root: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/drift_coverage_scan.ps1 -ScopePath . -SkipCoverage -SkipDependencyDrift` (`advisory` unless enrolled as required by owning team).
- [ ] Validate enforcement inventory gaps are closed: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/check-harness-enforcement-inventory.ps1 -FailOnGap` (`required`).
- [ ] Validate remediation log freshness and overdue-item policy: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/check-harness-remediation-log.ps1 -MaxReviewAgeDays 14` (`required`).
- [ ] Review harness coverage snapshots for gaps in maintainability, architecture fitness, behavior, and operability controls (`required`).
- [ ] Validate operational capability enrollment and evidence freshness: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/check-operational-harness.ps1` (`required`). Use `-RequireMature` only for a maturity-promotion decision.
- [ ] Review active execution plans, completed plans, decision records, and technical-debt entries for stale or superseded guidance (`required`).
- [ ] Review top recurring failures and create harness updates when a pattern repeats (`required`).
- [ ] Update [`harness-remediation-log.md`](harness-remediation-log.md) with owner, due date, and closure evidence for each finding (`required`).
- [ ] Run a scoped semantic documentation comparison using [`doc-gardening.md`](doc-gardening.md), and record corrections or an explicit no-finding result (`advisory` until enrolled as required).
- [ ] Capture and bounded-query the enrolled docs observability source, including access, redaction, correlation, retention, and runtime-to-harness evidence (`required`).
- [ ] In an `always()` final step, restore the latest maturity ledger, append the terminal run outcome, validate it, and retain it for 365 days (`required`).
- [ ] Exercise recovery for an enrolled autonomous workflow, including stale-lock or ambiguous-mutation reconciliation and diagnostic replay (`quarterly advisory`; failures create required remediation).
- [ ] Run `.github/workflows/ai-harness-agent-resource-and-cost-trend-review.yml` and enforce each workflow's rolling 7-day ceiling (`weekly required`).

## Optional Sensors

- [ ] Inferential review (`advisory`): run on recent high-risk changes and record output.
- [ ] Drift/coverage sensor (`advisory` unless enrolled as required by owning team): run dead code scan, coverage trend, and dependency drift check; alert on drops or new dead code.

## Failure Policy

- Pull request required checks are blocking.
- Main and scheduled jobs are blocking for regressions in required checks.
- Required checks must not be "skipped forever"; workflow design must resolve to success/failure for required statuses.
- Any new recurring failure mode or review comment repeated more than twice should result in a harness update:
  - guide update in docs/rules, or
  - sensor update in hook/script/test coverage.

## Ownership

- Primary owners: maintainers for the changed solution or project.
- Update this checklist when verify commands, test paths, or policy gates change.

## Functions deployment layouts and runtime

Digital-only deployment templates are supported layouts, not legacy exceptions. An app project can declare `<DeploymentTemplateLayout>digital-only</DeploymentTemplateLayout>` when its deployment pipelines use the digital templates exclusively. The deployment checker then requires `Template/azuredeploy_digital.json` and `Template/azuredeploy_digital_slot.json` instead of their standard counterparts. Environment parameters and other required assets remain checked. Do not declare digital-only while a standard deployment pipeline still depends on missing standard templates.

`docs/deployment-asset-exceptions.json` records genuine gaps in required assets, including CheckIn's existing standard-slot pipeline. It does not make the missing deployment path safe to use.

For .NET 10 isolated Functions apps, the deployment checker also validates configured `FUNCTIONS_WORKER_RUNTIME` literals, ARM parameter defaults, and environment parameter values in deployment JSON. These must use `dotnet-isolated`; ARM references to runtime parameters are checked through their defaults and supplied environment values. The guard does not execute ARM expressions or prove the deployed Azure configuration. `scripts/invoke-deployment-projects-exercise.ps1` covers supported layouts and rejected runtime settings.
