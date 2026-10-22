# Account-linking configuration — complete deployment table

Delivery status reconciled **13 October 2026**. The four-setting implementation and matching deployment cleanup merged in [PR #2325](https://github.com/VAAD1ST/eapi/pull/2325) on 5 October. The delivery owner confirmed DEV deployment; this is not independent runtime acceptance or evidence of SIT/UAT deployment. Secret presence and enabled/disabled status were verified on 2 October after cleanup. Secret values, trusted-caller comparisons and existing Redis configuration remain the **28 September snapshot**, not fresh value or deployed-runtime verification. All environment cells below are historical observations; verify current values and vault-reference resolution before activation.

The agreed journey requires **four account-linking setting names**, plus existing shared Redis/authentication/signing settings. The unrequested signed linked-token API and its four settings have been removed, together with the unused transaction-audience setting. No external assertion signer, public key set or application-level Redis encryption key is required.

Location abbreviations: **I** = Identity vault; **G** = GraphQL vault. Full secret name = environment prefix plus table suffix.

- DEV: I = `avadeunkveapis`, prefix `AVADEUNAFNIDENTITYMICROSERV-Config-`; G = `avadeunkventgraphql`, prefix `AVADEUNAFNGRAPHQL-Config-`.
- SIT: I = `avateunkvems`, prefix `AVATEUNAFNIDENTITYMICROSERV-Config-`; G = `avateunkventgraphql`, prefix `AVATEUNAFNGRAPHQL-Config-`.
- UAT: I = `avaseunkvems`, prefix `AVASEUNAFNIDENTITYMICROSERV-Config-`; G = `avaseunkventgraphql`, prefix `AVASEUNAFNGRAPHQL-Config-`.

| Configuration | Used for / source | Location / secret suffix | DEV | SIT | UAT | Example | Required for agreed flow? |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `RedisConnectionString` | Existing shared Validation Redis connection | I / existing `AVA{D/T/S}EUNAFNVALIDATIONMICROSERV-Redis-Connectionstring` | Configured | Configured | Configured | `example.redis.cache.windows.net:6380,password=<secret>,ssl=True` | Yes; pre-existing |
| `AccountLinking:TrustedCallerApplicationId` | GraphQL application allowed to create proofs; obtain from existing EAPI app registration | I / `AccountLinkingTrustedCallerApplicationId` | Configured; differs from authorised party | Configured; matches authorised party | Configured; matches authorised party | Approved GraphQL application ID | Only where different from existing authorised party; retain DEV override |
| `AccountLinking:TransactionAssertion:ClientId` | Exact approved linking client identifier from the agreed proxy/Alexa registration; legacy configuration name, no assertion | I / `AccountLinkingTransactionClientId` | **Empty; disabled** | **Empty; disabled** | **Empty; disabled** | `example-alexa-client` | Yes |
| `AccountLinking:TransactionAssertion:Resource` | Exact approved MCP resource identifier from the agreed integration; legacy configuration name, no assertion | I / `AccountLinkingTransactionResource` | **Empty; disabled** | **Empty; disabled** | **Empty; disabled** | `urn:example:mcp` | Yes |
| `Mcp:CallerContext:ApplicationId` | Approved calling application's Entra client ID for opt-in protected REST/MCP operations | G / `McpCallerContextApplicationId` | **Empty; disabled** | **Empty; disabled** | **Empty; disabled** | `22222222-2222-4222-8222-222222222222` | Yes for protected REST/MCP surfaces |

The first proof establishes a server-owned window of at most **120 seconds** with **three replacements**. Scope is fixed to `account:read`. No lifetime, replacement, external transaction signer or browser-key configuration is needed for the agreed journey.

## Deployment gates and cleanup

- Populate the approved client, resource and caller application ID; verify roles and existing OAuth/broker signing/Identity connectivity. Do not invent external registration values.
- Preserve the trusted-caller restriction. Created secrets are not necessarily configured; configured secrets are not deployed evidence.
- Retired secret suffixes: `AccountLinkingTransactionAudience`, `AccountLinkingTransactionIssuer`, `AccountLinkingTransactionJwks`, `AccountBindingAssertionAudience`, `AccountBindingAssertionApplicationId`, `AccountLinkingFieldEncryptionKeys`. All **18 DEV/SIT/UAT entries were soft-deleted on 2 October 2026**, with active-list absence verified; recoverable until 31 December 2026, not purged. Retained settings and shared infrastructure secrets were untouched. Use the matching cleanup merged in #2325 or later, not earlier revisions that still require removed secrets.
- Identity uses the existing cache service's per-call `SingleUse` mode; no global mode or extra setting is introduced. Redis payloads use existing EAPI GZip compression helpers, not application-level encryption. The delivery owner accepts reliance on resource security; Redis read access permits decompression. Keep TLS, least-privilege access and diagnostic redaction. This is not an audit of Azure security settings.
- The payload format is incompatible with earlier encrypted records. Before switching an active environment, stop proof creation on all old instances, let the maximum 120-second transaction window expire, then replace all instances. Do not mix old/new versions or flush shared Redis. Older implementations cannot be assumed deployable against the cleaned vaults; recovery of deleted secrets is available if an older version must be restored.
- Resolve shared Redis `volatile-lru` eviction/continuity risk before activation. Run live atomicity, replacement, expiry and strict replay tests on dedicated Redis.
- Existing application token issuance/validation and profile endpoints are unchanged. The summary consumes ordinary tokens; the unrequested signed linked-token issuance/validation endpoints are removed. Ordinary validation continues to reject previously issued linked-purpose tokens.
- Verify vault-reference resolution, deploy and complete environment acceptance. PROD Azure state has not been audited or changed.
