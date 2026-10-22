# Restore the agreed external account-linking contract

- Status: in progress
- Owner: EAPI
- Created: 2026-10-02
- Scope: Account GraphQL domain, Loyalty Identity, tests, examples and delivery tracking

## Objective

Honour the external contract in the original account-linking diagram at `b107b0e8e881b08ab0c6961864059908a9e01ee1`, as explicitly requested by the delivery owner. Do not require external callers to manufacture new transaction assertions, browser proofs or account-binding assertions to follow that journey.

## Plan

1. Restore proof creation/redemption inputs, outputs and single-use semantics with EAPI-owned validation, expiry and binding.
2. Reuse ordinary Flying Club token issuance and validation for the agreed journey; keep unrelated existing operations unchanged.
3. Update contract tests, Bruno examples, service/configuration guidance and the goal diagram.
4. Run scoped tests, TRX analysis and verification; raise a draft PR and update parent issue 75 and stories 176–179 with actual evidence and remaining activation gates.

## Decisions

- Plain identifiers are correlation/binding data, not evidence of external transaction authenticity or consent. The proxy continues to own OAuth state, PKCE, consent, account association and refresh/revocation.
- EAPI derives the customer account from the authenticated principal, accepts only the configured client/resource and fixed `account:read` scope, binds stored proofs to those values and requires authorised application redemption.
- Preserve encrypted Redis storage, server-controlled 120-second expiry, replacement limits and atomic one-time consumption. An exact second redemption must fail as the original contract specifies.
- Do not modify generic GraphQL/REST routing or existing sign-in, OTP, token issuance and profile contracts. Additional signed linked-token operations are not required by the restored journey; avoid silently changing their existing security contract.
- No Key Vault deletion or deployment is part of this change. Configuration documentation must distinguish the agreed flow from additional signed linked-token capabilities.

## Validation

- Account GraphQL focused tests: 54 passed, zero failed; TRX `TestResults/restore-contract-20261002-graphql-v5/graphql.trx`. Analysis found no failures. Coverage collection covered 14.20% lines / 10.58% branches across the broad instrumented dependency set; these are not feature-only coverage percentages.
- Identity focused tests: 115 passed, zero failed, nine dedicated-Redis cases skipped; TRX `TestResults/restore-contract-20261002-identity-v3/identity.trx`. Analysis found no failures. Build passed with 36 warnings and zero errors. The project's MTP runner ignored VSTest arguments and its generated executable could not launch; the built DLL was run through `dotnet` with native MTP filters and TRX reporting. Coverage was not available for this focused MTP run.
- Mermaid rendered successfully using the already-cached Mermaid CLI (the wrapper could not find `npx`); sequence-diagram status check passed.
- Required `scripts/verify.ps1 -ProjectPath ... -SkipBuild` ran for Account GraphQL and Identity; both exited 1 on repository harness requirements for absent `.github/workflows/ai-harness-*` / `ai-delivery-sequence-goal-*` files. These workflows are outside this contract correction; no full-green gate is claimed.
- No deployment, Azure secret deletion or external end-to-end acceptance is claimed. Live Redis concurrency/expiry acceptance and resolution of the existing eviction risk remain required before activation.

## Follow-up: remove unrequested signed linked-token API (2 October 2026)

The delivery owner authorized removal after the original restoration retained it. Repository inspection found no production caller required by the agreed flow. Remove the GraphQL operation, Identity issuance/validation endpoints, dedicated signed-assertion validator, encrypted retry outcome store, four settings and obsolete examples/tests. Keep existing ordinary issuer/validator behavior, including rejection of linked-purpose tokens. Keep proof encryption and atomic consumption. No generic GraphQL/REST host changes. This is an intentional removal of the additional API; external consumers cannot be established from repository references. Azure secrets remain untouched until deployed consumers are checked. Focused validation:
- Account GraphQL: 52 passed, zero failures; TRX `TestResults/remove-linked-20261002-account-v2/account.trx`. Coverage: 14.06% lines / 10.51% branches across all instrumented dependencies (not feature-only).
- GraphQL REST/MCP: 47 passed, zero failures; TRX `TestResults/remove-linked-20261002-rest-v2/rest.trx`. Coverage: 5.01% lines / 4.70% branches across instrumented dependencies.
- Identity: build succeeded; 79 passed, zero failures, nine dedicated-Redis tests skipped; TRX `TestResults/remove-linked-20261002-identity-v2/identity.trx`. Native MTP DLL execution used; coverage unavailable for this run.
- All three TRX analyses found no failures. Mermaid rendering, sequence status, deployment JSON parsing and diff whitespace checks passed.
- Retain existing redaction for the retired endpoint path so stale sensitive requests cannot become diagnostic payloads. It exposes no API, adds no configuration and avoids a shared-connector behavior change.
- Removed source and test fixtures remain recoverable in Git; no commits/pushes or Azure resource deletions performed for this follow-up.
- Scoped verification was invoked for Account, Identity and REST tests; repository-wide harness/workflow and sparse-checkout path failures prevent a full-green result. Corrected this goal's shorthand behavior sensor paths; remaining missing sensors are outside this goal.

## Follow-up: reuse compression and resource security (2 October 2026)

The delivery owner explicitly accepts Redis resource access controls and TLS instead of application-level field encryption. Remove the AES-GCM protector/interface, key-ring configuration and rotation/fingerprint logic. Reuse existing EAPI GZip compression helpers with a base64 payload inside Lua's JSON envelope. Keep hashed proof keys, fixed expiry, same-account checks (SHA-256), replacement limits and atomic redemption. Compression is not a confidentiality guarantee against Redis readers. Four account-linking-specific settings remain. No cloud resources or secrets are modified. For an active environment, stop creation on all old instances and drain the 120-second transaction window before replacing them; no mixed-version reads or shared cache flush. Earlier encryption decisions above are superseded by this instruction.

Validation: Identity test-project build passed (zero errors); focused native MTP run passed 72 tests with eight live-Redis cases skipped. TRX `TestResults/compress-linking-20261002-identity/identity.trx` analysis found no failures. Coverage was unavailable for the native MTP run. Compression round trips use the existing Core Xunit DistributedCache registered through DI; live Redis tests also inspect stored compressed payloads and reject replacement by a different account but require a configured dedicated cache to execute. Mermaid rendering passed. Scoped `scripts/verify.ps1 -ProjectPath ...Loyalty.Identity.csproj -SkipBuild` was run; repository-wide missing harness/workflow files prevent a full-green result. Changes remain uncommitted; deleted files are recoverable in Git.

## Follow-up: opt-in single-use mode on the existing cache (2 October 2026)

User requested reuse of DistributedCacheService with default behaviour preserved. Add ISingleUseDistributedCacheService as an additive interface, CacheMode per call on SetAsync and explicit TryConsumeAsync. Keep the original interface, constructor and Get/Set/Refresh/Remove implementations unchanged. Default mode delegates to the legacy Set; single-use mode requires Redis and propagates failures. Shared Lua handles generic binding/owner matching, absolute windows, policy-limited replacements and one-winner consumption; Identity supplies transaction scope, same-account binding, 120-second window and replacement limit. No application names/roles are embedded in the shared cache. Isolate single-use records from normal cache keys. Identity repository becomes a small policy adapter. No configuration is added. Redis integration tests are required to establish actual atomicity; cache mocks only cover default compatibility/failure behaviour. No commits, deployment or Azure deletion authorized by this follow-up.
Validation for shared mode: Identity and Infrastructure test-project builds passed. Identity focused tests: 72 passed, eight Redis cases skipped (`TestResults/single-use-20261002-identity/identity.trx`). Shared cache tests: three passed, two Redis cases skipped (`TestResults/single-use-20261002-cache/cache.trx`). Both TRX analyses found no failures; native MTP coverage unavailable. Actual concurrent Redis/expiry acceptance is not claimed without a dedicated Redis test connection. Mermaid rendered and sequence status passed. Scoped verify was run for Infrastructure and Identity with SkipBuild; repository-wide missing workflows, sparse-checkout sensors and harness state prevent a full-green result. Original DistributedCacheService method bodies and IDistributedCacheService interface are unchanged.
The shared-code preferred-extension gate initially flagged a built-in whitespace helper. Replaced it with the existing extension and reran the gate successfully; Infrastructure test-project rebuild passed. Final cache TRX: `TestResults/single-use-20261002-cache-final/cache.trx` (three passed, two dedicated-Redis cases skipped). Other listed harness failures remain outside this change.
