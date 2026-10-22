# GraphQL REST application-caller opt-in

## Native startup hook packaging

The REST project imports `../NativeWebSocketHost.targets` so build and publish output includes
`hostextensions/VAA.EAPI.GraphQL.WebSocketHost.dll`. The REST pipeline validates the merged package
contains a readable hook assembly before archiving it. Do not disable the required hook to work
around a missing publish artifact.

The DEV REST public and secure apps expose their deployed content under `C:\home\site\wwwroot`.
The REST build adjusts only its copied DEV deployment parameters to use this path, preserving
the enabled flag, other hook configuration, and the shared GraphQL parameter files. Both slot
and non-slot REST deployments consume these corrected artifact parameters. Rebuild and deploy
the new artifact; rerunning deployment with an old artifact cannot supply the missing DLL.

This corrects startup packaging, not WebSocket subscription integration: REST does not currently
implement the primary host's `/api/internal/graphql-websocket` callback. Host startup and HTTP/MCP
health must be checked separately from end-to-end subscription acceptance.

## Deployment order and DEV recovery

REST infrastructure deployments pass `PublishFunctionKey=false` so ARM does not call
`listKeys(.../host/default)` while the old host may be unhealthy. After package deployment
and startup (or a completed slot swap), the pipeline publishes the same default host key
only after an HTTP 200 from the deployed host's `/api/heartbeat` (up to 12 attempts,
10-second request timeouts and 5 seconds between attempts; a five-minute task timeout
also bounds hostname resolution and network delays). Failure blocks publication. The key goes
to the existing `functionappkey-<environment>` secret using `publish_function_key.json`.
Failures remain blocking; no keys are logged or emitted as deployment outputs.
Other users of the shared ARM templates retain immediate publication by default.
Build a new artifact when adopting this change; earlier artifacts lack the new template/parameter.

Both the live app and deployment slot identities need secret-read access to every referenced
vault before startup/swap. The infrastructure template's application-vault policy does not
grant access to a different vault referenced by an app setting. In DEV, the secure REST app
and DS slot use the existing `GraphQL-Native-WebSocket-Bridge-Secret` in
`avadeunkventgraphql`. Recovery on 14 October 2026 granted those two system-assigned identities
only secret `Get` access there; the secret was not replaced. Recreated identities require the
same access to be reprovisioned. Keep the Key Vault reference, hook and credential validation enabled.

All loaded GraphQL operations retain their generated REST routes. Operations marked with `McpCallerContextRequired` require the existing authenticated application bearer on both MCP and their generated REST route. The bearer must identify the configured backend application and satisfy the operation's declared roles. The marker and internal feature names remain unchanged for loaded-domain compatibility; the feature now carries the same authenticated principal, not a second JWT.

Send the normal `Authorization: Bearer ...` header. Shared authentication still validates its signature and lifetime. The opt-in boundary additionally checks the configured Entra v1/v2 issuer, exactly one configured GraphQL audience, the backend `azp`/`appid`, and absence of delegated `scp`. It uses the authenticated feature rather than trusting an unvalidated token. Missing or invalid application identity fails the existing router/tool authorization gate. Unmarked REST operations do not acquire this requirement; existing MCP header/subpath detection and public operations are preserved.

The host reads operation metadata only; it contains no domain-specific operation or role names.

The existing `Authorization: Bearer <application-token>,Token <broker-token>` convention remains supported, including the shared middleware's primary-token whitespace normalization. Issuer, audience, application and scope checks use the signature-validated principal, not a fresh decode of either raw token. The secondary token stays with the existing authentication/domain pipeline. Legacy MCP calls without a caller-context header already passed through the old caller-context middleware; this change does not introduce a new blanket authentication requirement for those calls.

| Configuration | Required after this change |
| --- | --- |
| `Mcp:CallerContext:ApplicationId` | Yes: the backend Entra application/client ID (existing setting retained). |
| Existing AAD issuer/tenant and `EAPIGraphAppID` settings | Reused unchanged from bearer authentication. |

`X-EAPI-Caller-Context` is no longer required or interpreted; clients still sending it remain compatible when their application bearer is valid. REST verbs, paths, GraphQL operation signatures, roles and response payloads are unchanged. No global authentication or router behavior was rewritten.

Security trade-off: this removes the additional request-signing credential and transport assertion replay check, aligning caller authentication with normal EAPI OAuth. Application bearers remain reusable until expiry. This is not mTLS or sender-constrained-token authentication; the removed static thumbprint comparison did not establish certificate possession. Identity still verifies transaction consent, signed account binding, proof expiry, encrypted storage and atomic redemption/replay prevention. Do not remove those domain controls. Review this authentication trade-off before rollout; deployment owner is the GraphQL service owner, with security review required before release. A rollback to the previous caller-signing design requires separately restoring its retired configuration before deploying the old build; those settings are not part of the active inventory.
