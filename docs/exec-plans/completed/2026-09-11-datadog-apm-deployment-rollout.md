# Datadog APM Deployment Rollout

- Status: completed
- Owner: Codex
- Created: 2026-09-11
- Scope: Existing Datadog-enabled ARM deployment templates and their environment parameter files across EAPI

## Objective

Every EAPI Function App deployment template that already exposes the shared `DataDog` logging configuration also exposes Datadog APM trace and runtime-metric settings, without enabling Agent log collection or changing extension installation.

## Context

EAPI sends application logs directly to the Datadog Logs Intake API when `LoggingProvider` selects Datadog. The platform now needs independent automatic Datadog traces and runtime metrics, while avoiding a second log collection path and retaining the existing direct logger for the future Application Insights retirement.

## Acceptance Criteria

- Every existing Datadog-enabled ARM template configures `DD_API_KEY`, `DD_SITE`, `DD_SERVICE`, `DD_ENV`, `DD_VERSION`, `DD_TRACE_ENABLED`, and `DD_RUNTIME_METRICS_ENABLED`.
- No template configures `DD_LOGS_ENABLED`.
- No template configures a Datadog site extension, `WEBSITE_PRIVATE_EXTENSIONS`, or slot-sticky setting plumbing; this preserves compatibility with legacy deployment pipelines.
- Matching environment parameter files supply the new shared `DataDog` fields and preserve Key Vault API-key references.
- All modified template and parameter files parse as JSONC; deployment templates pass contract checks and project/harness verification where available.

## Plan

1. Inventory existing Datadog-enabled templates, their app-setting representations, slot support, and parameter files. Completed.
2. Apply the standard APM settings to all matching templates and parameter files, without adding a site-extension resource. Completed.
3. Parse all changed JSONC files, check the repository-wide contract, run deployment-project/harness verification, and record the outcome. Completed.

## Decisions

- 2026-09-11: Extend only templates that already have the EAPI `DataDog` logging configuration. Adding monitoring to unrelated legacy deployments is a separate service-enablement change.
- 2026-09-11: Do not set `DD_LOGS_ENABLED`; EAPI retains its direct Logs Intake path and the Agent path could duplicate ingestion.
- 2026-09-18: Do not deploy `Datadog.AzureAppServices.DotNet` through these templates. The legacy pipeline path cannot reliably install it, and the extension must instead be introduced through a separately coordinated, stopped-app rollout.

## Validation

- Inventory and contract check: 205 Datadog-enabled deployment templates, 420 matching environment parameter files, and 78 deployment-slot templates. All parse as JSONC and expose the seven required APM settings; no template exposes `DD_LOGS_ENABLED`.
- Compatibility contract check: no changed template configures `Datadog.AzureAppServices.DotNet`, `WEBSITE_PRIVATE_EXTENSIONS`, or `ExistingSlotStickyAppSettingNames`.
- `git diff --check` passed.
- Representative deployment builds passed:
  - `VAA.EAPI.Microservices.Configuration.Deployment.deployproj`
  - `VAA.EAPI.Microservices.CRM.Content.Deployment.deployproj`
  - `VAA.EAPI.NDC.Offer.Deployment.deployproj`

## Outcome

The Datadog-enabled Function App deployments now carry the configuration required for traces and runtime metrics, but do not enable Agent log collection. The existing direct Datadog Logs Intake integration remains controlled by `LoggingProvider` until Application Insights is retired. Automatic instrumentation still requires a separately coordinated Datadog extension rollout.
