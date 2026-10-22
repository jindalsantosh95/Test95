# Stitch subscriptions on Azure Functions

Stitch executes subscriptions through its Hot Chocolate 13.9.15 schema. `RemoteSubscriptionTypeInterceptor` installs subscribe resolvers for remote subscription root fields using the schema's delegation metadata. Queries and mutations retain the existing HTTP stitching pipeline. No Concierge resolver or GraphQL host project is referenced by Stitch.

The native host hook uses **bridge** mode: client WebSocket -> Stitch host -> internal host dispatch -> Stitch worker/executor -> schema-specific downstream WebSocket. `NativeSubscriptionRegistry` is compiled from shared source in `../NativeWebSockets`; it does not introduce a host project dependency. GraphQL currently uses Hot Chocolate 12 and Stitch uses 13, so the shared source retains the version-specific subscription-error check for the primary host.

This remains an experimental Functions host extension. It has five-minute sessions and no replay. Native subscriptions always use [Redis](../VAA.EAPI.GraphQL.WebSocketHost/README.md#redis-scale-out-configuration) to distribute bridge results across workers and domain events across GraphQL instances. It is not an Azure Functions WebSocket trigger.

## Configuration

Configure these values before starting the Functions host. Keep existing unrelated local settings.

```json
{
  "NativeWebSocketsRedisPrefix": "local-stitch",
  "FUNCTIONS_WORKER_PROCESS_COUNT": "1",
  "NativeWebSocketsBridgeSecret": "<locally generated random value of at least 32 characters>",
  "NativeWebSocketsPath": "/api/v1/GraphQL/ws",
  "Graphql:Endpoints": "general=http%3A%2F%2Flocalhost%3A7071%2Fapi%2Fv1%2FGraphQL"
}
```

- Configure the existing `RedisCacheConnectionString` through ConfigReader for both applications; no separate Redis resource is required.
- There is no enable flag. Worker subscription services are always registered. Loading the startup hook through `DOTNET_STARTUP_HOOKS` activates the WebSocket route; unloading it provides transport rollback.
- These entries belong inside `Values` in `local.settings.json`; do not replace the rest of that file.
- `DOTNET_STARTUP_HOOKS` must contain the absolute path to Stitch's built `hostextensions/VAA.EAPI.GraphQL.WebSocketHost.dll`. Start Visual Studio/Core Tools with this host environment configured. Remove obsolete `NativeWebSocketsMode` and `NativeWebSocketsTarget` settings from local launch profiles.
- By default, subscription URLs are derived from `Graphql:Endpoints`: change HTTPS to WSS (HTTP to WS on loopback only), retain host and port, trim a trailing slash and append `/ws`. For example, `https://example.com/dev/graphql/public` becomes `wss://example.com/dev/graphql/public/ws`. Query strings, credentials and fragments are rejected for derived URLs.
- `Graphql:Endpoints` is the only endpoint configuration. HTTP and derived WebSocket routes must reach the **same GraphQL application** using the same Redis namespace. There is no separate subscription URL override.
- Use WSS in Azure; unencrypted WS is permitted only for loopback development. Do not put credentials in endpoint URLs.
- The hook dispatches bridge commands internally through the Functions-host middleware, which invokes the isolated worker normally. There is no loopback or HTTPS self-callback and no dependency on `WEBSITE_HOSTNAME` for the bridge. Remove obsolete `NativeWebSocketsBridgeViaAppHost` settings; they are no longer read.
- Store the bridge secret in Key Vault and grant the Stitch app identity access. The secret authenticates the internal host-to-worker callback; it is never forwarded downstream.
- Remove `NativeWebSocketsTarget` and any old pipeline override for it. Deployment templates no longer accept it; Stitch derives URLs from its HTTP schemas, with optional per-schema overrides.

Both Mobile and standard Stitch templates (including slots) expose the secure `NativeWebSocketsBridgeSecret` parameter; the transport always uses the worker bridge. Separate subscription endpoint parameters and settings have been removed. Verify the deployed HTTP schema names/backend alignment and grant the Stitch identity access to that secret before deployment. Previously deferred hook activations remain unloaded.

## DEV deployment prerequisites

The former Mobile DEV subscription override has been removed. Deploying this version derives the route from the current HTTP `general` endpoint. Before deployment, ensure that APIM exposes that exact path with `/ws` appended and routes it to the same GraphQL app as HTTP. A historical `/dev/GraphQL/secure/ws` route does not substitute for a different derived path. No live Key Vault or APIM changes are made by this code change.

The DEV bridge secret reference uses the existing `avadeunkventgraphql` vault. Confirm the Mobile app identity can resolve `GraphQL-Native-WebSocket-Bridge-Secret`; an unresolved reference must fail closed. Deploy the new Stitch package and settings together. The APIM client-facing WebSocket API must target **Stitch**, while the downstream `general` WebSocket API must target GraphQL, otherwise the stitched executor is bypassed or a routing loop is created.

## APIM routes for automatic URL derivation

APIM must expose a WebSocket API at the derived path; changing the URI scheme in Stitch does not create an API or convert the existing HTTP API into a WebSocket API.

For an HTTP schema endpoint of `https://testresources.virginatlantic.com/dev/graphql/public`, configure a WebSocket API with path `dev/graphql/public/ws` and WSS protocol. Set its backend to the **same app as the HTTP API**, at `/api/v1/GraphQL/ws`. Associate the appropriate GraphQL product and retain its subscription-key requirement. Forward `graphql-transport-ws` and the caller credentials. The client-facing Stitch WebSocket API separately targets the Stitch app.

Remove obsolete `Graphql:SubscriptionEndpoints` app settings and `NativeWebSocketsSubscriptionEndpoints` deployment overrides. They are no longer read or accepted by templates. The DEV `VAA.EAPI.GraphQL.Mobile.Deployment/Configure-DevWebSocketRoute.ps1` script previews the Public GraphQL and client-facing Stitch WebSocket routes; pass `-Apply` to reconcile them with the existing HTTP APIs. It preserves policies and follows each HTTP API's subscription-key requirement. It does not change Function App settings or Key Vault permissions. Confirm the HTTP schema map still matches these APIs before running it.

## Test through Stitch

1. Start primary GraphQL on port 7071 and Stitch on 7072 through the Functions host. Use matching `general` HTTP/WS endpoints as above.
2. In Postman connect to `ws://localhost:7072/api/v1/GraphQL/ws` with `Sec-WebSocket-Protocol: graphql-transport-ws`.
3. Send `{"type":"connection_init"}` immediately; expect `connection_ack`.
4. Send:

   ```json
   {"id":"ping-pong","type":"subscribe","payload":{"query":"subscription { conciergeMessageReceived(conversationId: \"11111111-1111-4111-8111-111111111111\") { messageId message response } }"}}
   ```

5. POST to **Stitch** at `http://localhost:7072/api/v1/GraphQL` with `Content-Type: application/json`:

   ```json
   {"query":"mutation { postConciergeMessage(conversationId: \"11111111-1111-4111-8111-111111111111\", message: \"ping\") { messageId message response } }"}
   ```

6. Confirm the WebSocket `next` event contains `pong` and the mutation's `messageId`. Send `{"id":"ping-pong","type":"complete"}` to release the downstream subscription.

For Azure, use the APIM HTTP and WebSocket APIs that both front Stitch and include the required APIM subscription key. Anonymous POC fields need no bearer token. When supplied, credentials are forwarded per subscription to the owning GraphQL application's authentication middleware. Stitch does not treat a forwarded token as a locally authenticated identity.

## Supported scope and verification

Root subscription delegation, generated root-field renames/argument mappings, aliases, variables, fragments, structured results and downstream GraphQL errors are supported. Arbitrary nested `@delegate` paths and scoped context-variable mappings are rejected rather than silently routed incorrectly. Hot Chocolate's existing field middleware remains active. Each subscription uses its own downstream socket to avoid sharing credentials across callers.

The new `VAA.EAPI.GraphQL.Stitch.Tests` project uses synthetic local Hot Chocolate servers to test HTTP mutation delegation, schema-specific WebSocket selection, validation, event projection, authentication forwarding/rejection, callback credentials and cancellation. Existing Stitch/Mobile pipeline test globs already select this project. Real Azure/APIM validation is still required after deployment.

## SSE subscriptions

Stitch also supports POST subscriptions over SSE alongside WebSockets. See [SSE setup and testing](../SSE.md) for local payloads, downstream routing, APIM provisioning and deployment limitations. No new app settings are required.
