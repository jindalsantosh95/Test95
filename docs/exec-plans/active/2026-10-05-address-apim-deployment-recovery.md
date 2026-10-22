# Address Lookup deployment cleanup and v2 authentication alignment

- Status: implementation and offline verification complete; live rollout pending
- Owner: Account / Loyalty Address Lookup and Platform Engineering
- Created: 2026-10-05
- Scope: Remove #2071's extra v3 authentication/deployment dependencies and correct the existing shared APIM cleanup

## Objective

Use v2's existing server-to-server authentication for v3 instead of adding a signed-context token, Function key or APIM Key Vault dependency. Preserve the two-step cost-saving contract, existing v2 behavior and deployment entry points. Remove the emergency app-stop fallback without introducing a replacement deployment design.

## Context

DEV/SIT logs show APIM Key Vault secret-read denial. The cleanup introduced in #2071 could then stop the whole Function App, affecting v2 and slot swaps. The user explicitly requested no new vault, no extra deployment machinery, and no extra authentication layer absent from v2.

V2 already uses GraphQL's application OAuth token for `AddressLookupAPIAppId` and server-side `EnrolmentMSOcpKey`. Enrolling users do not supply those credentials. The corrected v3 uses that same path. No live environment is changed by this PR.

## Acceptance Criteria

- No new deployment job, helper scripts, permission grants, agent-IP discovery or inline Swagger contract.
- Preserve the existing shared APIM job, pipeline entry points and environment stages.
- Remove emergency stop/restart behavior; preserve original errors, targeted web-configuration PATCH writes and failure/cancellation cleanup.
- GraphQL v3 uses the existing application token provider and subscription configuration. No user token/session requirement is introduced.
- Both service triggers use v2's standard authentication middleware and OAuth2 metadata, with no additional Functions host key or signed-context validation.
- Remove the six v3 APIM Key Vault named-value resources and their settings; explicitly replace the old API policy with inherited base policies.
- Retain encrypted references, expiry/type/country validation, Find-only suggestions and one Retrieve per selected address with no automatic retry.
- Update fixture-backed tests, deployment contract checks, Bruno examples and owning documentation.

## Decisions

- 2026-10-05: Initial recovery proposal introduced a service-owned job, Key Vault permission provisioning, agent-only Swagger retrieval and new fixtures.
- 2026-10-05: User requested removal of that proposal from #2343. Earlier 20-scenario results describe removed code and are superseded.
- 2026-10-05: Correct the existing shared cleanup generically. All its consumers lose the emergency-stop fallback; no address-specific shared branch is added.
- 2026-10-05: Retain the existing temporary-access/live-Swagger publication pattern. A cleanup failure can leave access incorrect and needs immediate operator recovery, not an app stop.
- 2026-10-05: The initial decision to retain six APIM named values and seek secret-read permission is superseded by the user's request to follow v2. Remove the custom v3 layer rather than grant APIM new vault permissions. Existing service/provider/reference secrets continue to use the existing vault and identity.
- 2026-10-05: Keep empty APIM environment parameter files and their existing pipeline wiring. This preserves the opted-in production APIM stage so the old policy is replaced everywhere. Deleting only the ARM policy resource would not remove the already-deployed policy in incremental mode.
- 2026-10-05: References are no longer caller-class/dataset/session-bound; any holder able to invoke public GraphQL can replay them until expiry. Payload version `v2` rejects old references; outer token format remains `v1`. The implementation request is not production approval of replay/abuse controls.

## Validation

- Address Lookup microservice: **48/48** focused v3 tests passed, including v2/v3 auth metadata parity, reference-version rejection, no-context selection, Find-only suggestions, encoded provider calls, telemetry and no automatic Retrieve retry.
- Account GraphQL: **32/32** v2/v3 address tests passed, including v2 regression, existing server OAuth/subscription, no custom headers, structured input, local/hosted configuration and failed-auth/no-dispatch.
- REST routing: **6/6** tests passed; validation retains 400/503 and both operations remain discoverable.
- Current-run TRX analysis for all three suites reports no failed tests.
- The Windows environment blocks the generated xUnit v3 apphost. Microservice tests built successfully, then ran through the supported `dotnet <test-assembly>.dll -class "*V3*" -trx <run-path>` in-process runner. Its focused run has no coverage collection; the failed apphost run is not counted as coverage evidence.
- Account and REST runs used `dotnet test --no-restore -m:2 -c Release --filter <focused-filter> --results-directory <run-path> --logger "trx;LogFileName=results.trx" --collect:"XPlat Code Coverage"`.
- `scripts/test-apim-access-cleanup.ps1`: **10 offline scenarios passed**, no Azure calls.
- `scripts/check-address-lookup-v3-apim.ps1`: all four environments passed; standard subscription and inherited policies, no retired APIM vault/context settings, no middleware bypass.
- Scoped `scripts/verify.ps1 -Base origin/main -SkipBuild` passed with normal permissions for its temporary Git/file fixtures; GraphQL and microservice test-standard checks and GraphQL preferred-extension checks passed.
- Mermaid CLI 11.12.0 rendered the updated sequence successfully; knowledge-health, sequence-status, behavior-harness and whitespace checks passed.
- No live Azure deployment, permission changes, provider charges or DEV/SIT smoke tests performed.

### Coverage reported by the current-run analyser

These are focused-test aggregates across instrumented assemblies, not feature-only coverage or full-suite claims.

| Suite | Line coverage | Branch coverage | Lines | Branches |
| --- | ---: | ---: | ---: | ---: |
| Account v2/v3 address | 10.62% | 7.76% | 1165/10970 | 410/5281 |
| Address v3 REST routing | 5.12% | 3.77% | 651/12705 | 230/6107 |
| Address v3 microservice | Not collected | Not collected | In-process runner fallback | Apphost blocked |

Current evidence paths: `TestResults/address-v2-auth-micro-inprocess-final/results.trx`, `TestResults/address-v2-auth-graphql-verified/results.trx`, and `TestResults/address-v2-auth-rest/results.trx`. These ignored local artifacts are not committed.

## Rollout and remaining risks

Deploy GraphQL first, then coordinate the service and APIM policy replacement: the old gateway strips Authorization, while the new service requires v2's application token. Mixed versions can return 401; this is not a zero-downtime compatibility claim. Fetch fresh suggestions after deployment.

Use a fresh pipeline run. Recover previously stopped apps/DS slots after checking approved restrictions; both must be running before a swap. This change removes the new APIM secret-read dependency, but does not repair stopped state, incorrect IP restrictions or unrelated environment permissions. Verify v2 address details, v3 suggestions/container expansion and an approved selection after deployment.

Incremental deployment leaves old unused named values and secrets in place; no deletion or new vault is performed here. Separate logging/TLS/cache hardening and production replay/abuse approval remain owner concerns. Do not claim live recovery or measured savings from these offline checks.
