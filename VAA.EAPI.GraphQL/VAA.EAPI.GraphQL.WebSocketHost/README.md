# Experimental native Functions WebSocket transport

This review preview keeps both applications running through the Azure Functions SDK. A .NET startup hook adds a WebSocket route to the Functions host's ASP.NET Core pipeline. It does not replace the Functions entry point or run another worker-side web server. This is an unsupported host extension technique, not a supported Azure Functions WebSocket trigger. Validate it against the actual Azure host before adopting it.

## Execution path

Client -> APIM WebSocket API -> Stitch host hook -> credential-protected internal Stitch dispatch -> stitched Hot Chocolate executor -> schema-specific downstream WebSocket -> primary GraphQL host hook and worker executor.

- The primary hook implements `graphql-transport-ws` for subscription operations. Queries and mutations continue through the existing HTTP Function.
- The worker creates a real HotChocolate subscription, including validation, variables, operation selection and result projection. Fifteen-second long polls carry results back to the socket handler. These are additional Function invocations.
- The callback always requires the internal bridge credential. Authorization is optional on the socket and is forwarded unchanged when supplied; the existing Functions authentication middleware handles it and GraphQL field authorization decides access. Each session is bound to a server-generated connection ID and its validated principal (or anonymous state). The callback is excluded from response caching.
- Stitch bridge mode executes subscriptions through its schema and opens a downstream socket for the owning remote schema. It shares only transport/session source and does not reference the primary host or domain projects. Both applications always use the worker bridge. See [Stitch setup](../VAA.EAPI.GraphQL.Stitch/README.md).
- Authenticated Concierge topics are scoped by caller and conversation. Publish with the same caller token used for the subscription. Anonymous Concierge events are public to anonymous subscribers sharing that conversation ID; the ID is not an authorization boundary. Protect private fields using the existing GraphQL authorization rules.

## Prerequisites and limits

Native subscriptions use Redis for both one and multiple Windows App Service instances and worker processes. No single-instance acknowledgement or mode switch is required. Each instance must reach Redis and, for Stitch, the downstream GraphQL transport endpoint.

The socket and worker session last at most five minutes. Reconnect and resubscribe; events are not replayed. Each socket allows eight active operations, and the worker allows 128 sessions. Messages must be text JSON: inputs are limited to 64 KiB, outputs to 1 MiB, and initialization must arrive within ten seconds. This preview does not implement the legacy `graphql-ws` protocol or queries/mutations over WebSocket.

The hook creates a fresh internal POST request for the fixed /api/internal/graphql-websocket route and passes it to the downstream Functions-host middleware. Routing, function authorization and isolated-worker dispatch run normally; no HTTP client, loopback listener or HTTPS self-callback is used. Each command has independent request features, a service scope, completion callbacks and cancellation. Only approved caller headers and the bridge credential are forwarded; outer principals, endpoints and administrative features are not inherited. Internal command and response buffers are bounded. Host and worker remain separate processes connected by the normal Functions runtime.

The .NET startup-hook contract and ASP.NET Core hosting-startup mechanism are runtime extension points, but their combination here is not an Azure Functions compatibility guarantee. Revalidate whenever the Functions host updates. The locally tested Core Tools runtime is recorded in the execution plan; `netFrameworkVersion=v10.0` in app configuration describes the worker target, not necessarily the host runtime.

## Configuration

Both projects package `hostextensions/VAA.EAPI.GraphQL.WebSocketHost.dll` through `NativeWebSocketHost.targets`. Build or publish normally. Set these environment variables **before starting the Functions host**, not only the worker process. For local testing, launch `func start` from a shell containing these variables and the usual app configuration.

| Setting | Primary | Stitch |
| --- | --- | --- |
| `DOTNET_STARTUP_HOOKS` | Absolute deployed hook DLL path | Absolute deployed hook DLL path |
| `NativeWebSocketsRedisPrefix` | Unique app/environment namespace, e.g. `dev-graphql-public` | Different namespace, e.g. `dev-stitch-public` |
| `RedisCacheConnectionString` | Existing Redis connection, resolved through ConfigReader | Connection to the existing Redis resource |
| `FUNCTIONS_WORKER_PROCESS_COUNT` | Configured worker count, one or more | Same |
| `NativeWebSocketsBridgeSecret` | Random secret of at least 32 characters; Key Vault reference in Azure | Required for the Stitch host-to-worker callback |
| `OperationsVisibilityMode` | Existing schema visibility configuration (`Public` exposes the anonymous Concierge POC; `Secure` excludes it) | Existing configuration |

The default route is `/api/v1/GraphQL/ws`; override with `NativeWebSocketsPath` if necessary. Requests with a query string are rejected. The application does not enforce an Origin allowlist. Apply browser-origin restrictions at APIM when required.

For Visual Studio executable launch profiles, put every applicable `NativeWebSockets*` setting from the table, `FUNCTIONS_WORKER_PROCESS_COUNT`, and `DOTNET_STARTUP_HOOKS` in the profile's `environmentVariables`. The hosting-startup extension runs before Core Tools imports `local.settings.json`; putting only the hook path in the profile leaves the early transport configuration incomplete. Both primary and Stitch need their Redis prefix and callback secret in that early environment. Stitch derives all subscription URLs from `Graphql:Endpoints`; APIM must expose each derived route. Keep secret-bearing launch profiles ignored and local. Rebuild after changing ordinary local settings so the output copy is refreshed.

Resolve the actual deployed Windows content path before configuring the hook. Do not assume a particular drive letter. Preserve existing instrumentation hooks in `DOTNET_STARTUP_HOOKS`, using the runtime's platform path-list separator (`;` on Windows). An invalid hook path can prevent the whole host from starting. Keep the bridge secret out of source control and logs.

The primary, Mobile and Stitch ARM templates configure the platform WebSockets switch and transport app settings. There is no enable flag. Loading the DLL through `DOTNET_STARTUP_HOOKS` activates the route. Worker subscription services always require Redis configuration. Existing deferred PROD parameter files leave `startupHookPath` empty pending acceptance; Public Stitch and the historical Test5 pipeline also leave the hook unloaded. Digital and Sandbox put transport settings in their authoritative child `appsettings` resource. No launch profile is deployed.

Before enabling an environment, update its parameter file (or the deployment task's explicit parameter overrides):

| ARM parameter | Required value |
| --- | --- |
| `NativeWebSocketsConfiguration.startupHookPath` | `D:\home\site\wwwroot\hostextensions\VAA.EAPI.GraphQL.WebSocketHost.dll` in the Windows environment files; override for a different verified content path |
| `NativeWebSocketsConfiguration.existingStartupHooks` | Current hook list, excluding this transport DLL, or empty if none; semicolon separated |
| `NativeWebSocketsConfiguration.workerProcessCount` | Configured worker count, one or more |
| `NativeWebSocketsConfiguration.path` | Defaults to `/api/v1/GraphQL/ws` |
| `NativeWebSocketsBridgeSecret` | Primary and Stitch: Key Vault reference resolving to a random secret of at least 32 characters |

These non-secret settings share the `NativeWebSocketsConfiguration` object parameter to stay below ARM's 256-parameter limit. Supply the complete object when overriding it; its properties retain the same runtime app-setting names listed above. The secure bridge credential remains a separate parameter. There is no transport mode setting; the raw proxy implementation has been removed. They append the native DLL to `existingStartupHooks` when the hook path is nonempty and use only that existing list when the path is empty. These are desired-state settings: populate the existing hook list before deployment rather than assuming ARM will discover and preserve portal-only settings. Keep the hook path separate from the existing list to avoid loading it twice.

Example primary parameter entries (the checked-in environment files already contain their own vault references):

```json
"NativeWebSocketsConfiguration": {
  "value": {
    "startupHookPath": "D:\\home\\site\\wwwroot\\hostextensions\\VAA.EAPI.GraphQL.WebSocketHost.dll",
    "existingStartupHooks": "",
    "workerProcessCount": "1",
    "path": "/api/v1/GraphQL/ws"
  }
},
"NativeWebSocketsBridgeSecret": { "value": "@Microsoft.KeyVault(SecretUri=https://<vault>.vault.azure.net/secrets/GraphQL-Native-WebSocket-Bridge-Secret/)" }
```

For Mobile and standard Stitch deployments, supply the same common parameters, `NativeWebSocketsBridgeSecret`. The obsolete `NativeWebSocketsTarget` parameter and app setting have been removed; remove any old pipeline overrides too. The bridge secret parameter is `securestring`; environment files store only the Key Vault reference, never the actual secret. The existing app identity needs access to resolve it. Provision `GraphQL-Native-WebSocket-Bridge-Secret` with a random value of at least 32 characters in each referenced vault before deployment; these changes add references, not secret values or vault resources.

Stitch derives downstream WebSocket URLs from its HTTP schema map. Verify the derived routes exist and reach the same application as HTTP. The Windows path uses the App Service D: mapping described in [Microsoft's filesystem guidance](https://learn.microsoft.com/en-us/azure/app-service/operating-system-functionality); it still needs verification on the target host. Apply the correct values separately to each app/slot before restart. Some pipelines reuse one environment parameter file for both Secure and Public apps: use per-deployment overrides where their values differ. Slot templates include the same settings; review the target URL and slot-sticky configuration before any swap. This change does not create slots, resize plans, provision resources or add APIM WebSocket APIs. Changing app settings restarts the host; validate the DLL exists in the deployed package before enabling the hook.

### DEV verified-path deployment

The **GraphQL** pipeline uses a two-phase activation flow for its `dev` Secure and Public apps. **GraphQL Stitch** uses it for DEV Secure only. Public Stitch explicitly disables the native transport in every environment and does not run activation: the shared `general` subscription endpoint points to the Secure primary and must never bypass public-schema visibility. Other environment stages and slot/digital deployment pipelines do not use automatic activation. The existing DEV apps are restarted during deployment; this is not a zero-downtime rollout.

1. Before ARM deployment, `scripts/deploy-graphql-native-websockets.ps1 -Phase Prepare` reads the target app's current hook list. It writes temporary parameters with native transport disabled and preserves unrelated hooks, including any configured in the parameter file. It does not snapshot live secrets to disk.
2. The existing ARM/package/start tasks deploy normally, without loading the native DLL during host startup. The activation task explicitly requests loading the verified hook; the checked-in drive-specific hook path is ignored by this DEV flow.
3. After package deployment, `-Phase Activate` uses the existing service connection to run a read-only PowerShell command through Kudu. It resolves the remote `HOME` directory, checks the DLL in `site\wwwroot\hostextensions`, and compares its SHA-256 hash with the deployment ZIP. A missing DLL, inaccessible SCM endpoint or mismatched package prevents activation.
4. The pipeline appends the verified **absolute** path to the preserved hook list, enables platform/native WebSockets, sets the configured worker count and restarts the app. It waits for an anonymous `graphql-transport-ws` `connection_ack` from the app's direct WSS route. Relative paths and literal `%HOME%` are not passed to .NET.
5. If activation/restart/handshake fails after settings have changed, it restores the post-ARM settings with the native hook removed and WebSockets disabled, restarts, and fails the job. This is a safe disabled transport baseline, not a rollback of the deployed application package. If ARM or package deployment fails earlier, activation does not run. An interrupted agent or failed Azure rollback requires manual recovery using the rollback steps below.

Run the GraphQL DEV deployment before Secure Stitch so its configured WSS backend is available. Public Stitch requires an explicitly configured public-schema backend and acceptance coverage before separately enabling its transport. Verify Redis access and internal bridge execution on every participating app before scaling. App settings do not change plan capacity.

The deployment identity needs app/config read/write/restart permissions and SCM publishing access; the agent needs network access to both SCM and the app's WSS endpoint. This uses bearer authentication, not publishing passwords, and does not enable SCM basic authentication. See [Kudu access permissions](https://github.com/projectkudu/kudu/wiki/accessing-the-kudu-service) and [Kudu command API](https://github.com/projectkudu/kudu/wiki/REST-API). No new Azure resources or local Visual Studio paths are required.

The automated check proves transport initialization only. Complete the Concierge mutation/subscription test below, then repeat through APIM, before claiming end-to-end subscription readiness. The offline regression exercise is `pwsh -File scripts/test-graphql-native-deployment.ps1`.

### Historical proxy acceptance (14 October 2026)

These results cover the previous fixed-target proxy implementation, not the new stitched subscription executor. Use the [current Stitch setup](../VAA.EAPI.GraphQL.Stitch/README.md) for configuration and repeat acceptance after deployment.

Verified the deployed DLL and resolved `HOME` through SCM on both apps. The actual path is `C:\home\site\wwwroot\hostextensions\VAA.EAPI.GraphQL.WebSocketHost.dll`. Secure Digital GraphQL uses `bridge` mode with app-host callbacks; Mobile Stitch uses `proxy` mode. Both have platform WebSockets enabled and one worker. Both hosting plans had capacity one at validation.

- Primary: `wss://va-dv-eun-gqlsingleinstances-fn-01.avadeunase014.appserviceenvironment.net/api/v1/GraphQL/ws`.
- APIM: `wss://testresources.virginatlantic.com/dev/GraphQL/secure/ws`, forwarding directly to that primary.
- Stitch: `wss://va-dv-eun-graphqlmobile-fn-01.avadeunase014.appserviceenvironment.net/api/v1/GraphQL/ws`, forwarding to APIM. Supply an `Eapi-GraphQL` product key in `Ocp-Apim-Subscription-Key`; Stitch forwards it.
- Publish the POC mutation to `https://va-dv-eun-gqlsingleinstances-fn-01.avadeunase014.appserviceenvironment.net/api/v1/GraphQL`. Mobile Stitch's existing HTTP schema does not expose Concierge; it has not been widened to include the Secure schema.

Each path returned `connection_ack` and a real Concierge `next` event with response `pong`, matching the `messageId` returned by its HTTP mutation. The earlier workstation security-proxy 403 was no longer present during these tests. This verifies the DEV POC, not scale-out, replay, production readiness, or compatibility with future Functions host updates.

The Mobile deployment templates now include opt-in transport parameters, enabled only in the DEV parameter file. That historical version detected Azure app-host callback routing automatically; the current implementation uses internal dispatch instead. Merge and deploy these changes before another release to retain the callback fix. `VAA.EAPI.GraphQL.Mobile.Deployment/Configure-DevWebSocketRoute.ps1` configures a historical APIM backend API and product association; it no longer changes Stitch endpoints; it is a separate manual deployment step, not automatically run by the application pipeline. Do not point that APIM API back to Stitch, which would create a loop.

## Redis scale-out configuration

Native subscriptions reuse the existing Redis resource for two purposes: Hot Chocolate publishes GraphQL domain events across instances, and short-lived Redis mailboxes let bridge read/stop callbacks reach any worker. The subscription executor stays on its owning worker. Stitch continues executing its stitched schema and remote subscription resolvers.

Set the following on **both** GraphQL and Stitch, in addition to the existing hook, bridge and endpoint settings:

```json
"NativeWebSocketsRedisPrefix": "dev-graphql-public",
"FUNCTIONS_WORKER_PROCESS_COUNT": "2"
```

Use `dev-stitch-public` for Stitch's prefix. All replicas of one app must share its prefix; use different prefixes for other apps, environments and slots. Prefixes accept 3ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Â ÃƒÂ¢Ã¢â€šÂ¬Ã¢â€žÂ¢ÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã¢â‚¬Â¦Ãƒâ€šÃ‚Â¡ÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â¦ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã¢â‚¬Å“100 letters, digits, underscores or hyphens. Both apps need a resolved `RedisCacheConnectionString`, Redis network access and permission for Pub/Sub, hashes, lists, expiry and Lua scripts. Preserve TLS and existing cache configuration. The subscription multiplexer is separate from the existing cache connection; no new Azure resource is provisioned. Monitor shared Redis CPU, memory and connections before increasing capacity.

For deployment, optionally add `redisPrefix: "dev-graphql-public"` to the **complete existing** `NativeWebSocketsConfiguration` object; retain its path, hook and other properties. Set the desired `workerProcessCount`. By default templates derive the prefix from the unique Function App name, including slot name for slot deployments. Overrides must remain distinct between apps and slots. Mobile/Stitch templates accept `AppSetting:RedisCacheConnectionString` as a secure parameter with the existing vault reference. The app identity must be allowed to resolve it. Worker subscription services always require Redis, even with one instance. Activation refuses to proceed if the deployed namespace or connection is missing. Remove obsolete `NativeWebSocketsEnabled`, `NativeWebSocketsSingleInstance` and `NativeWebSocketsRedisEnabled` settings from local profiles/app settings; they are no longer read. The templates do not change App Service plan capacity or autoscale rules.

Each mailbox holds at most 128 frames and 1 MiB, expires after 330 seconds, and is bound to the validated caller and socket connection. Slow consumers receive an error and must resubscribe. A remote stop reaches the owner on its next poll (normally within 15 seconds); a crashed owner's lease is detected after 45 seconds. Sessions still last at most five minutes. Worker restarts and scale-in require clients to reconnect and resubscribe. Redis Pub/Sub and mailbox reads do not provide durable replay or exactly-once delivery; interruptions can lose events.

Validate with at least two instances/workers: subscribe through Stitch, publish a matching Concierge mutation on another GraphQL worker, verify the message ID and pong, then stop through a different callback worker. Also test scale-in and reconnect. Automated tests exercise independent service providers/registries against real Redis; Azure multi-instance acceptance remains required. Internal dispatch removes the self-callback network dependency; Redis and Stitch-to-GraphQL connectivity are still required. The Functions host hook remains an unsupported Azure extension.

## APIM

Keep the existing GraphQL HTTP API. Add a separate APIM WebSocket API targeting Stitch's `wss://.../api/v1/GraphQL/ws` route. Preserve `Sec-WebSocket-Protocol: graphql-transport-ws` and required authentication/correlation headers. Avoid HTTP response caching, body transforms and response buffering on this route. If APIM requires a subscription key, send it in the handshake header. Bearer credentials may be sent in the handshake Authorization header or connection initialization payload; APIM handshake JWT policies require the header; make those policies optional where anonymous operations are intended.

Do not publish `/api/internal/graphql-websocket` through APIM. Its OpenAPI operation is marked internal, but still explicitly exclude it when importing/filtering operations. HTTP OpenAPI attributes cannot describe or provision the WebSocket protocol. If Stitch's backend goes through APIM too, that hop also needs a WebSocket API; a normal HTTP GraphQL API is insufficient.

### Optional browser-origin policy (guidance only)

Origin restrictions belong to the client-facing APIM WebSocket API's `onHandshake` inbound policy. Its normal HTTP CORS policy does not apply to WebSocket handshakes. No APIM policies or live resources were changed by this implementation. Restrict direct Function App ingress if APIM is the enforcement boundary. Origin checks are separate from GraphQL authentication and authorization.

Example fragment to merge into an existing inbound policy, using the real approved website origins. This permits clients without Origin (such as Postman or server-to-server callers), but checks the header when supplied:

```xml
<choose>
  <when condition="@(context.Request.Headers.ContainsKey(&quot;Origin&quot;))">
    <check-header name="Origin" failed-check-httpcode="403"
                  failed-check-error-message="Origin not allowed" ignore-case="false">
      <value>https://approved-website.example</value>
    </check-header>
  </when>
</choose>
```

See Microsoft's [WebSocket policy support](https://learn.microsoft.com/en-us/azure/api-management/websocket-api) and [check-header policy](https://learn.microsoft.com/en-us/azure/api-management/check-header-policy). `NativeWebSocketsAllowedOrigins` and `NativeWebSocketsConfiguration.allowedOrigins` are no longer used.

## Postman acceptance test

1. Start primary and Stitch using normal Functions hosting with the settings above. Keep Stitch's existing HTTP endpoint configuration pointing to primary. For example, use primary port 7071 and Stitch port 7072.
2. Create a raw WebSocket request to `ws://localhost:7072/api/v1/GraphQL/ws` (or the APIM `wss://` URL). Add `Sec-WebSocket-Protocol: graphql-transport-ws`, an optional bearer token, required `api-*` headers and any APIM subscription key. Connect.
3. Send `{"type":"connection_init"}`. Expect `{"type":"connection_ack"}`. Alternatively put the bearer string in `payload.Authorization` when handshake policies allow it. Anonymous initialization is allowed. A 401/403 rejection from the existing middleware closes with 4403. GraphQL authorization failures are returned as operation errors; an acknowledgement is not permission to access protected fields.
4. Send this frame:

```json
{"id":"concierge-1","type":"subscribe","payload":{"query":"subscription Observe($id: UUID!) { conciergeMessageReceived(conversationId: $id) { messageId message response createdAtUtc } }","operationName":"Observe","variables":{"id":"11111111-1111-4111-8111-111111111111"}}}
```

5. In a separate HTTP GraphQL request to the existing Stitch endpoint, use the same authentication state (no token for anonymous, or the same bearer token) and conversation ID:

```graphql
mutation {
  postConciergeMessage(conversationId: "11111111-1111-4111-8111-111111111111", message: "ping") {
    messageId message response createdAtUtc
  }
}
```

6. Expect a `next` frame with `payload.data.conciergeMessageReceived.response` equal to `pong` and the same message ID as the mutation result. Send `pong` and expect `ping`. A transport-level ping/pong alone is not an acceptance test.
7. Send `{"id":"concierge-1","type":"complete"}` and confirm subsequent mutations do not deliver frames. Repeat with a different caller and confirm isolation. Repeat through APIM, then test five-minute reconnect, restart and cleanup.

If the first mutation races subscription creation, post again after a short pause; the protocol has no per-subscription acknowledgement. Inspect sanitized Function logs for callback failures, without recording tokens or frame bodies.

## Rollback

Remove only this DLL from `DOTNET_STARTUP_HOOKS` and restart both apps. Set `startupHookPath` empty in deployment configuration to keep it unloaded on the next release. Remove/disable the APIM WebSocket API if required. Worker subscription services and Redis configuration remain registered. No Web PubSub adapter or resource is used.

## SSE transport

The same host extension now installs the sibling /sse endpoint alongside /ws. It uses the existing authenticated worker bridge and Redis sessions. See [SSE setup and testing](../SSE.md). The assembly name is retained for existing deployment compatibility.
