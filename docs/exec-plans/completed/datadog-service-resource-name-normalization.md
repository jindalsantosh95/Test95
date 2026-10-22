# Datadog Service Resource-Name Normalization

- Status: completed
- Owner: Codex
- Created: 2026-08-28
- Scope: Repository-wide ARM templates that already configure Datadog

## Objective

Every ARM template in the repository that configures Datadog derives the `DatadogService` app setting from its Function App resource-name parameter.

## Context

Loyalty Identity emitted Datadog logs with the generic service name `loyalty-identity` despite its Function App resource name being environment-specific. The current templates map `DatadogService` from the independently maintained `DataDog.Service` deployment parameter.

## Acceptance Criteria

- Every existing `DatadogService` mapping in an `azuredeploy*.json` template uses the template's `FunctionAppName` or `appName` parameter.
- No generic `[parameters('DataDog').Service]` mapping remains in the repository.
- The redundant `Service` property is removed from every affected DataDog template default and environment parameter object.
- All changed templates remain valid JSON and resource-name parameters resolve correctly.

## Plan

1. Inventory all ARM templates containing a `DatadogService` app-setting mapping.
2. Replace each generic DataDog service mapping with the corresponding Function App resource-name parameter.
3. Validate all changed JSON files and scan for remaining generic mappings.

## Decisions

- 2026-08-28: Normalize only templates that already configure Datadog. Introducing Datadog into legacy templates without existing Datadog settings is a separate observability-enablement change.
- 2026-08-28: Use the Function App resource-name parameter for the Datadog service. This avoids duplicated environment-specific service values while retaining one service name for a production slot and its parent Function App.
- 2026-08-28: Extend normalization beyond `VAA.EAPI.Deployments` to the existing GraphQL, Lounge Management, NDC Transformation, Orchestration, and Web deployment projects so the repository has one convention.

## Validation

- Verified all 208 `DatadogService` mappings: 184 use `FunctionAppName` and 24 use `appName`.
- Parsed all 633 changed deployment JSON files; parser tolerance matched pre-existing repository files that contain comments or trailing commas, and this change introduces no new JSONC syntax.
- Confirmed no `[parameters('DataDog').Service]` mapping or redundant DataDog `Service` property remains anywhere in the repository.
- Ran `git diff --check`; the deployment diffs contain no whitespace errors.

## Outcome

All existing Datadog app-setting mappings in the repository now use their Function App resource-name parameter.
