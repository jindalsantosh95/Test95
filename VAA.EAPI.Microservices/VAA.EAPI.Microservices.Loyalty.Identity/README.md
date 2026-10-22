# Loyalty Identity: account linking

## Agreed external journey

The external contract is the original diagram at commit `b107b0e8e881b08ab0c6961864059908a9e01ee1`, restored by the delivery owner's instruction on 2 October 2026. See the [maintained EAPI goal](../../docs/sequence-diagrams/vaa-eapi-alexa-mcp-proxy-account-linking.md).

1. The custom login page uses existing Account GraphQL sign-in and OTP contracts.
2. Authenticated `accountCreateLinkingProof(input: AccountCreateLinkingProofInput!)` accepts `transactionId`, `clientId`, `resource`, `scope`; returns `proof` and UTC `expiresAtUtc`.
3. The authorised proxy calls `loyaltyRedeemLinkingProof(input: LoyaltyRedeemLinkingProofInput!)` with `proof`, `transactionId`, `clientId`, `resource`; receives `flyingClubAccountNumber` and `consentSubject`.
4. The proxy uses the existing role-protected `loyaltyIssueFlyingClubApplicationToken` with that verified account number, then existing profile access or the least-data `loyaltyFlyingClubAccountSummary`. The summary validates the ordinary broker token. Existing issuer/signing/authorised-party settings remain unchanged.

No signed transaction assertion, browser-key proof or account-binding assertion is required for this journey. The proxy owns OAuth transaction authenticity, state/PKCE, consent, account association, refresh and revocation. Plain identifiers bind EAPI's proof but do not attest to external consent; `consentSubject` is an opaque correlation identifier. EAPI is not an OAuth authorisation server.

The existing ordinary-token issuer trusts the authorised application's account-number input. It does not enforce proof-of-redemption continuity. The proxy must request tokens only for its verified linked account; this original trust boundary requires explicit deployment/security acceptance, not a claim that the removed signed-binding guarantees still apply.

## EAPI security and persistence

- Account GraphQL requires the existing authenticated customer token and resolves exactly one active Flying Club account server-side. Callers cannot select a customer account in the public creation input.
- GraphQL calls Identity using its existing application credentials. The scoped Identity controller requires operation roles and proof creation verifies the configured trusted GraphQL caller.
- Identity validates bounded, non-control-character transaction identifiers, exact configured client/resource and the fixed `account:read` scope. Missing policy fails closed. Configuration key names under `TransactionAssertion` for client/resource are retained to reuse existing deployment references; these two values no longer describe an assertion.
- EAPI generates a random 256-bit opaque proof, stores only its hash and compresses the account/consent binding using existing EAPI GZip helpers (base64 inside the Lua JSON envelope).
- Proofs expire within 120 seconds. The first creation establishes the transaction window; up to three replacements may occur without extending it. Replacement invalidates the previous proof. Responses report the remaining persisted proof expiry.
- Redis Lua checks the transaction binding and atomically consumes the current proof. Exactly one concurrent redemption succeeds. Every subsequent redemption, including an identical retry, fails. A mismatch does not consume the valid proof. A lost redemption response requires restarting the linking journey; do not automatically retry redemption.
- The shared Validation Redis connection is reused through `RedisConnectionString`, with TLS forced for this connection. Identity opts into the existing `DistributedCacheService` through `ISingleUseDistributedCacheService`, using `CacheMode.SingleUse` and `TryConsumeAsync`. Legacy `IDistributedCacheService` callers retain their read/refresh/error-suppression behaviour. Single-use mode uses Redis Lua and propagates dependency failures; it cannot fall back to separate `IDistributedCache.Get/Remove` calls.
- Application-level encryption and keyed account fingerprints are removed at the delivery owner's request. A SHA-256 owner-binding hash preserves same-account replacement checks without configuration. Compression is reversible by Redis readers; resource access controls and TLS provide the accepted security boundary. Raw account data and proofs must not be logged.
- Drain the maximum 120-second transaction window with proof creation stopped before replacing all old instances; earlier proof records are not read by the shared cache's isolated single-use namespace. Do not mix versions or flush shared Redis.
- The known shared Redis `volatile-lru` eviction/continuity risk must be resolved before activation. This PR does not change cache policy.

Only GraphQL implementations call internal microservice URLs: `POST /account-linking-proof`, `POST /account-linking-proof/redeem`, and the existing `POST /application-token` and `POST /application-token/validate`. Public REST and MCP exposure comes from the generic GraphQL REST harness, not an account-linking subrouter. Existing authentication middleware, sign-in, OTP, ordinary token and profile behaviour are unchanged.

## Removed unrequested signed-token API

The additional `loyaltyIssueLinkedFlyingClubBrokerToken` operation and internal linked-token issuance/validation endpoints have been removed by the delivery owner's instruction. The agreed journey reuses ordinary token issuance and needs no external account-binding signer, issuer, JWKS, binding audience or binding application setting. Its unused assertion validator, retry-outcome store and examples are removed too.

Ordinary issuance and validation keep their contracts and authorization. Ordinary validation still rejects linked-purpose tokens; removal must not make old linked tokens interchangeable. No in-repository production caller of the removed API remains; external consumer usage is not verifiable from this repository. The implementation removal is included in PR #2325. Azure cleanup is complete: 18 obsolete DEV/SIT/UAT secrets were soft-deleted on 2 October 2026, as recorded in the deployment table. Deploy the matching cleanup rather than an earlier revision.

## Configuration and validation

See the [complete configuration table](ACCOUNT-LINKING-DEPLOYMENT.md). The obsolete assertion settings and field-encryption key ring are removed from runtime constants and deployment templates/parameters; only four account-linking-specific settings remain. Removed vault secrets are recoverable until 31 December 2026. Retained client/resource/caller settings still require configuration; no deployment acceptance is claimed.

Bruno examples are in `VAA.EAPI.GraphQL.Domain.Account.Bruno/Account Linking` and `Secure Transactions`; internal proof examples are in the Identity Bruno collection. Never commit live credentials, account data or proof values.

Use real DI types and the existing core Xunit mocks. Run the owning Account and Identity tests with TRX analysis. For actual atomicity/replacement/replay tests, set `ACCOUNT_LINKING_REDIS_TEST_CONNECTION` to a dedicated test Redis. `ACCOUNT_LINKING_REDIS_NO_SCRIPT_TEST_CONNECTION` tests failure when scripting is denied. Readiness checks PING, SCRIPT LOAD, EVALSHA and EVAL; it does not prove continuity or key ACL correctness.

Deployment, configured role grants, vault resolution, Redis acceptance and the external login-to-account journey remain acceptance gates. Merged code is not evidence of deployment or external integration.
