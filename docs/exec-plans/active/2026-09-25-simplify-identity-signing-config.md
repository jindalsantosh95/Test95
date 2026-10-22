# Reuse account-linking backend signing configuration

- Status: ready for review
- Owner: Identity service owner; implementation by Codex
- Created: 2026-09-25
- Scope: Identity account-binding validator, focused tests, main/slot deployment defaults and README

## Objective

Remove the need to configure the same backend issuer and public keys twice, without changing existing configured trust boundaries. This branch starts directly from main and is independent of the GraphQL REST caller-authentication simplification.

## Acceptance Criteria

- Complete binding issuer/JWKS overrides retain existing behavior and precedence.
- Omitting both overrides selects the existing transaction issuer/JWKS as a pair; partially configured pairs fail closed.
- Wrong signatures, issuers, audiences and operation purposes remain rejected.
- No new setting names, no application/client identity consolidation, no changes to consent, proof storage or global authentication.
- Existing environment parameters are preserved; README documents an explicit migration and rollback.

## Decisions

- 2026-09-25: shared signing is selected only by omission of both binding overrides. Sharing keys is appropriate only when the backend owns both contracts. Separate audiences and operation validation remain mandatory. Service owner must confirm common signing ownership before changing deployment values.

## Validation

- Identity test project compiled in Release. The initial `dotnet test` invocation could not launch its generated executable (Windows access denied); VSTest logger/collector options were also ignored by the existing MTP runner.
- Ran the compiled test DLL using `dotnet .../VAA.EAPI.Microservices.Loyalty.Identity.Tests.dll --filter-class '*AccountBindingAssertionValidatorTests' '*LinkingTransactionValidatorTests' '*AccountLinkingProofHandlerTests' '*AccountLinkingProofRepositoryContractTests' --results-directory .../TestResults/signing-config-final --report-xunit-trx --report-xunit-trx-filename signing-config-final.trx`: 68 passed, 0 failed, 5 skipped. Skipped cases require an explicitly configured test Redis instance. No live Redis integration verification claimed.
- `scripts/analyse-test-run-results.ps1 -TrxPath TestResults/signing-config-final/signing-config-final.trx -SuiteName "Identity linking" -EnvironmentName local`: no failed tests. Coverage is unavailable: the installed MTP runner exposes no coverage extension.
- Main/slot template JSON parsing and `git diff --check`: passed.
- `scripts/verify.ps1 -ProjectPath VAA.EAPI.Microservices/VAA.EAPI.Microservices.Loyalty.Identity/VAA.EAPI.Microservices.Loyalty.Identity.csproj -SkipBuild`: exit 1 on repository-wide harness checks (missing out-of-scope automation/solution files in sparse checkout and harness metadata). Knowledge health, sequence status, preferred extensions and test standards passed.

## Outcome

- Pending review. No live settings or vault entries changed.
