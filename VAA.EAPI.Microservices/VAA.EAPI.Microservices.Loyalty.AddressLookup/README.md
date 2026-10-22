# Loyalty Address Lookup

## Address Lookup v3 suggestions and selected-address retrieval

PR #2071 delivers additive v3 HTTP endpoints, public Account GraphQL operations, APIM/deployment configuration, Bruno examples and backend tests. V2 remains supported and unchanged. Shared HTTP connectors remain unchanged: the Account v3 provider uses the existing non-generic connector overload with `noWait: true` to send a single no-follow header, then deserializes the response with the shared JSON serializer. General platform security changes are reviewed separately below. No consumer UI changes or measured production savings are delivered by this PR.

V2 returns address details through the existing Find/Retrieve flow. V3 returns suggestions from Find and performs a Retrieve only when an address is selected. V3 adds no result cache and never automatically retries a paid Retrieve. Existing platform cache settings remain in effect.

Suggestions automatically expand nested containers through Find in batches of at most five concurrent calls per request, then return only unique Address candidates with encrypted references. Container IDs are visited once per request (including cyclic/overlapping results); Address IDs are deduplicated before encryption. Loqate can return containers within containers, so expansion continues until no unvisited containers remain, subject to a hard maximum of 20 expansion calls plus the initial Find. If that budget is exhausted, the service returns `422 SearchTooBroad` asking the caller to narrow the search, not a partial success. An expansion failure fails the whole request without retries. Cancellation stops subsequent batches; already-dispatched calls may still be processed by the provider.

`AddressLookupV3SuggestionLimit` remains a per-Find limit, not a cap on the merged response. The existing default is six; the merged list may exceed six. This is not exhaustive postcode enumeration: provider result limits and fuzzy matching still apply. Expansion increases Find request count and latency but never invokes Retrieve. No extra configuration, caching or shared GraphQL changes are introduced.

Every provider item with a nonblank ID and nonblank type other than `Address` is expandable, following the [Loqate Find Container contract](https://docs.loqate.com/api-reference/address-capture/find). This includes locality, building-number and sub-building navigation levels without maintaining a closed type whitelist. Malformed items lacking an ID or type are ignored; the existing call budget and cycle detection still apply.

Each `AddressLookupV3Find` event describes only its individual provider call. Later expansion or reference-generation failures do not change a successful Find outcome. Those failures emit a separate `AddressLookupV3SuggestionProcessing` event with only `flowId`, `stage` (`expansion` or `references`) and `outcome` (`failed` or `cancelled`). A budget rejection is a processing failure, not a failed provider call.

The v3 selection request accepts only the selected Address candidate's `reference`. No client-generated selection ID is needed; the service derives its internal diagnostic identity from the validated reference. Existing valid Container/Postcode references remain accepted as suggestion inputs for compatibility; clients no longer need to expand containers returned by a new suggestion response.

References are encrypted, tamper-protected, and valid for at most five minutes. They are not caller-class, dataset or session-bound: any holder able to call the public GraphQL operation can use a reference until expiry. Do not log or share references. V3 uses v2's existing server-to-server authentication and inherited APIM controls, with no separate v3 rate-limit policy. References do not deduplicate selections.

## REST routing and cancellation

GraphQL suggestions also accept the existing address-details input field names: `countryCode`, `postCode` and `addressLine1`. Account combines the nonblank, trimmed address line and postcode into one Find query; the microservice HTTP contract remains unchanged. The original free-text `query` input remains supported. Do not combine address fields with `query` or a container `reference`. Container expansion remains reference-only, and full details are returned only by the selection mutation.

```graphql
query AccountAddressSuggestionsV3($request: AddressLookupV3SuggestionInput) {
  accountAddressSuggestionsV3(request: $request) {
    flowId
    candidates { type text description reference }
  }
}
```

Variables: `{"request":{"countryCode":"GB","postCode":"RH10 6DF","addressLine1":"4"}}`. The v3 input type is separate from `AddressDetailRequestInput`; the v2 operation and its full-address response are unchanged.

The existing GraphQL REST host discovers both public v3 operations as `POST /api/v1/accountAddressSuggestionsV3` and `POST /api/v1/accountAddressSelectionV3`; no v3-specific exclusion remains. Use the normal REST request envelope, for example `{"request":{"query":"London","countryCode":"GB"}}` or `{"request":{"reference":"<fresh Address reference>"}}`. Existing host configuration and capability discovery apply; this does not bypass the downstream APIM boundary.

Both v3 service operations document HTTP 408 with the `Cancelled` error response. Cancellation after provider dispatch does not prove that no charge occurred; do not automatically retry selection. Suggestion references are created before success is recorded, so reference-configuration failures are recorded as failures rather than deferred serialization errors.

## GBG configuration

Both versions read the existing Key Vault-backed `GbgServiceKeyMap`, using the `GBGConfig` shape (`ClientId`, `GbgKey`). V2 retains its application-client lookup and default fallback. V3 always uses the first entry with an empty or whitespace `ClientId`, matching v2's no-token fallback. The default entry must contain a non-empty `GbgKey`. Missing, malformed, or incomplete configuration returns `503 ProviderKeyUnavailable` before a v3 provider request.

There is no separate `GbgV3KeyMap` or provider credential mapped by caller class/dataset. The retired signed caller context is no longer used for reference binding or telemetry. V3 retains `GbgV3FindEndpoint` and `GbgV3RetrieveEndpoint` because it constructs and percent-encodes query parameters independently of the legacy v2 URL templates.

Reusing a provider credential mixes v2 and v3 in provider key-level totals. V3 Find/Retrieve telemetry helps diagnose requests, but may be sampled or lost and is not an authoritative billing ledger. A provider-billed v3 ratio requires reporting that separates v3 operations from v2; shared-key billing totals alone cannot prove it.

## Local v3 testing

Configure the service's ignored `local.settings.json` with the existing default entry in `GbgServiceKeyMap`, both v3 provider endpoints, and the remaining `AddressLookupV3` settings: minimum query length, suggestion limit, reference lifetime and encryption key.

For localhost, set GraphQL's `AddressLookupV3:ApimUrl` to the service's loopback URL. Keep the same `AddressLookupAPIAppId`, OAuth application credentials and `EnrolmentMSOcpKey` used by v2. Loopback is allowed only with `AZURE_FUNCTIONS_ENVIRONMENT=Development` and no `WEBSITE_INSTANCE_ID`. There is no local signing mode or v3 context key. The service uses the same authentication middleware as v2, including its existing debugger exception. No client `api-session-id` or end-user token is required. Restart after configuration changes and fetch fresh suggestions. Never commit credentials or local settings.

## Deployment and authentication alignment (2026-10-05)

At the user's request, v3 now follows v2's existing application-authentication path. GraphQL acquires an application token for the existing `AddressLookupAPIAppId` through `IOAuthTokenProvider`, and sends it alongside the existing `EnrolmentMSOcpKey`. These are server-side credentials, not an enrolment-user token or new client subscription requirement. Missing server credentials prevent dispatch. The dedicated v3 subscription setting remains retired.

Both v3 Functions use `AuthorizationLevel.Anonymous` plus the standard `AuthenticationMiddleware` and OAuth2 OpenAPI metadata, matching v2. Anonymous here means no additional Functions host key, not an unauthenticated backend. Existing APIM subscription validation and network restrictions remain. The custom signed-context validator, Function-key injection, local signing settings and caller-class/dataset claims are removed.

The APIM template explicitly replaces the old v3 policy with inherited `base` policies. Simply omitting the policy resource would leave the old policy stripping Authorization in incremental deployments. All six v3 Key Vault-backed APIM named-value resources and parameters are removed; APIM no longer needs to read those v3 secrets. No new Key Vault, permission grants, identities or secrets are introduced. The service's existing provider credentials and reference-encryption key remain in the existing environment vault.

The empty environment APIM parameter files and existing pipeline wiring are retained so every environment, including the existing opted-in production stage, publishes the policy replacement. Incremental deployment does not delete previously created named values or vault secrets; they become unused and any cleanup is a separate owner action.

**Coordinate the GraphQL, service and APIM rollout.** Deploying GraphQL first is compatible with the old gateway, but the new service needs the bearer token that the old APIM policy strips. Service and gateway versions must be switched together during a controlled deployment; mixed versions can return 401. Reference payload contract version changes to `v2` (the outer encrypted format remains `v1`), so clients must obtain fresh suggestions after deployment. Old references fail validation before GBG dispatch. Offline tests do not establish live environment recovery or production security approval.

GraphQL rejects structured fields combined with a query/container reference with `400`. Missing/blank server subscription or application token returns `503`, with numeric `httpStatusCode` and `transportStatusCode` extensions for existing REST routing. Neither dispatches to Address Lookup. No automatic Retrieve retry is introduced.

At the user's request, v3-specific cache bypasses and `no-store` headers have been removed. A matching existing cache configuration can store v3 responses. No cache rules are added or changed by this removal. Both the GraphQL and microservice v3 telemetry processors have also been removed. The new opt-in shared logging-suppression mechanism and v3 call sites have been split out of this PR at the user's request. The pre-existing microservice URI flag remains, but does not cover all completion/error logs; GraphQL connector failures can include request/response bodies. Neither host has the additional v3 automatic Application Insights filter. Production rollout requires separate logging hardening and explicit v3 adoption or owner-approved equivalent controls; the split is not security approval. Standard telemetry and request-count diagnostics remain.

The shared HTTP TLS/cache hardening and GraphQL host logging/authorization changes are split into independent platform PRs, not delivered by this address PR. Shared factories, cache keys/entries, gateway query logging and policy handling therefore retain `main` behaviour. In particular, the legacy certificate-validation bypass and potentially credential-bearing cache/log data are not fixed by v3. Production rollout needs owner-reviewed platform fixes or approved equivalent safeguards; the scope split is not security approval. No v3 cache bypass or automatic paid Retrieve retry is introduced.

Separate reviews: [TLS/cache security #2308](https://github.com/VAAD1ST/eapi/pull/2308), [GraphQL host security #2309](https://github.com/VAAD1ST/eapi/pull/2309), and [opt-in HTTP logging #2306](https://github.com/VAAD1ST/eapi/pull/2306).

## Audit storage removed (2026-10-01)

At the user's request, v3 no longer writes dispatch/outcome audit records or runs audit-retention/recovery timers. Selection goes directly from reference validation to a single GBG Retrieve; it does not require audit storage, audit credentials, retention settings or Azurite Table storage. No paid Retrieve is automatically retried.

Find and Retrieve events retain safe flow/selection/attempt correlation in Application Insights. These events are diagnostic only: sampling, telemetry outages or a process crash can lose evidence. There is no durable recovery or authoritative per-selection billing denominator. Provider-billed savings need independently validated v3-attributed reporting; shared-key totals include v2 and cannot establish the v3 ratio.

Existing audit tables, rows and Key Vault secrets are not deleted by this code change. After deployment the removed retention timer will no longer clean up historical rows. The service owner must arrange retention or disposal of existing data separately; unused audit settings can be removed from local configuration without replacing any current settings.

## Review follow-up (2026-09-30)

Find events include the server-issued `flowId`; Retrieve events also carry the validated `selectionIntentId` and per-attempt `dispatchId`. These are restricted diagnostic correlation properties, not aggregate metric dimensions. These application events exclude payloads, provider IDs, credentials and references; that does not imply shared logs or automatic telemetry have comprehensive redaction. GraphQL's Account domain owns the address settings; shared GraphQL files contain no v3 operation checks. Its address-specific telemetry processor, factory and registration have been removed.

PR #2071 added an emergency stop when the shared APIM cleanup failed. That could disable existing v2 traffic and prevent subsequent slot swaps. The correction removes that fallback from the existing shared `deployment_apim.yaml`, keeps unconditional cleanup and validation, preserves the original Azure error, and targets access-setting writes at `Microsoft.Web/sites/config` (`web`) instead of writing the entire site resource. This generic correction applies to all consumers of that shared job; it contains no address-specific branch. `scripts/test-apim-access-cleanup.ps1` exercises the actual inline steps with offline fakes.

## APIM deployment recovery (2026-10-05)

At the user's request, the recovery PR retains the existing deployment approach instead of introducing another pipeline. The proposed Address Lookup job, permission-provisioning scripts, IP-discovery dependency, temporary agent-IP rules and captured-Swagger import have been removed. Both existing pipeline entry points call the existing shared APIM job. The existing stages publish the simplified authentication contract described above, including the explicit replacement of the old API policy.

The v3 APIM-to-Key-Vault dependency that produced the logged secret-read failure is removed rather than granting APIM new permissions. This does not repair already-stopped apps, incorrect access rules, or unrelated service-identity permissions. Existing provider/reference secrets continue to use their current vault and service identity.

The pre-#2071 Swagger publication pattern temporarily opens backend access and restores default deny/APIM access afterwards. That existing pattern is not redesigned here. Cleanup runs on failure/cancellation, but a control-plane or lockdown failure can still leave access incorrect: the job fails with the original error and requires immediate operator intervention rather than stopping the app. A narrower network publication design belongs in a separately approved platform change. Do not run concurrent deployments against the same app.

### Recover an environment already affected

1. Use a fresh build containing this corrected shared job; old pipeline runs still contain the emergency-stop logic.
2. Verify and restore the approved APIM allow rule and default deny after any failed cleanup. Check the target backend and gateway IP when investigating `Ip Forbidden`; do not leave Allow All enabled.
3. Explicitly start any app or DS slot stopped by the old job, after verifying restrictions. Both must be running before a swap; this change does not automatically restart them.
4. Deploy the coordinated GraphQL/service/APIM versions above and verify the active API policy inherits the v2 path without signed-context headers or Key Vault named-value references.
5. Verify v2 `accountAddressDetails`, then v3 suggestions/container expansion and an approved selection. Offline checks do not establish live recovery.
