# Simplify account-linking application caller authentication

- Status: ready for review
- Owner: GraphQL service owner; implementation by Codex
- Created: 2026-09-25
- Scope: GraphQL REST opt-in caller boundary, tests, deployment settings and client example

## Objective

Reuse authenticated Entra application tokens instead of requiring a second signed caller-context JWT, certificate claim and dedicated transport replay cache. This branch starts directly from main and does not depend on the separate Identity signing-configuration simplification.

## Acceptance Criteria

- Existing shared authentication, REST/MCP routes, roles and public operation contracts remain unchanged.
- Opted-in operations require an authenticated application principal, configured issuer/audience/application binding and operation roles.
- Invalid/missing identity cannot acquire the feature required by the router/tool gate.
- Remove only transport assertion infrastructure; retain Identity consent, account binding, encryption and atomic proof consumption.
- Update deployment wiring, README and Bruno example; run focused regression tests and scoped verification.

## Decisions

- 2026-09-25: user requested independent simplification PRs and preservation of existing behavior. Retain marker/feature names to avoid cross-domain contract churn; derive the feature from shared authentication.
- Security trade-off: normal bearer-token reuse replaces independent request signing. The prior static certificate-claim comparison was not proof of certificate possession. Service owner/security reviewer must approve this trade-off before deployment; no live settings are changed here. If approval is not obtained, do not deploy this PR. Rollback retains the old secrets and redeploys the previous build.

## Validation

- `dotnet test VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Rest.Tests/VAA.EAPI.GraphQL.Rest.Tests.csproj -c Release --no-restore --results-directory TestResults/caller-auth-regression --logger "trx;LogFileName=caller-auth-regression.trx" --collect:"XPlat Code Coverage"`: 66 passed, 0 failed/skipped.
- `scripts/analyse-test-run-results.ps1 -TrxPath TestResults/caller-auth-regression/caller-auth-regression.trx -SuiteName "GraphQL REST regression" -EnvironmentName local`: no failures; aggregate dependency-inclusive coverage 7.70% lines / 6.95% branches.
- Deployment JSON parsing and `git diff --check`: passed. Shared AuthenticationMiddleware, McpMiddleware and Router have no code diff.
- `scripts/verify.ps1 -ProjectPath VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Rest/VAA.EAPI.GraphQL.Rest.csproj -SkipBuild`: exit 1 on repository-wide harness checks (missing out-of-scope automation/solution files in sparse checkout and harness metadata). Knowledge health, sequence status, preferred extensions and test standards passed. No end-to-end environment deployment or mTLS verification claimed.
- Repeated the scoped verification for `VAA.EAPI.GraphQL.Common.csproj`: same repository-wide failures; sequence status, preferred extensions and test standards passed.

## Outcome

- Pending review. No changes to existing global authentication middleware or Identity proof controls.

## Review follow-up (25 September)

- Corrected stale Account Summary Bruno guidance. Preserved the existing application-plus-broker-token Authorization header and whitespace normalization in the new caller middleware; authorize using the already validated principal claims instead of re-decoding raw tokens. No edits to shared authentication, routing or domain signatures.
- The proposed blanket gate for unmarked MCP traffic is out of scope: the main version already calls next when the assertion header is absent. Introducing that gate would break the expressly preserved legacy behavior. Ignoring obsolete assertion headers is the documented simplification trade-off.
- Full REST tests rerun with real signed/validated JWT principals: 68 passed, zero failures/skips. TRX analysis (`TestResults/review-compatibility/review-compatibility.trx`) reported no failures; aggregate line/branch coverage 7.72%/7.07%.
- REST-scoped `verify.ps1 -SkipBuild` rerun after the fix: same repository-wide sparse-checkout/harness failures (exit 1); knowledge health, sequence status, preferred extensions and test standards passed. `git diff --check` passed.
- Follow-up issuer-root finding: high severity, in scope. Existing deployment authorities include a trailing slash. Added six v1/v2 canonical-issuer cases (no suffix, `/`, `///`), each also rejecting an unrelated issuer. Four cases failed before the fix. Normalize only the configured roots in the new middleware; shared authentication and deployment values stay unchanged. Full REST suite after the fix: 74 passed, zero failures/skips; TRX analysis at `TestResults/issuer-root-green/issuer-root-green.trx` reports no failures, with aggregate line/branch coverage 7.72%/7.10%.
