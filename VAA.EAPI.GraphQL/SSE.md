# GraphQL subscriptions over SSE

GraphQL and Stitch expose two subscription transports alongside the existing HTTP query/mutation endpoint:

| Transport | Local route | Purpose |
| --- | --- | --- |
| HTTP JSON | `/api/v1/GraphQL/` | Existing queries and mutations |
| WebSocket | `/api/v1/GraphQL/ws` | Existing `graphql-transport-ws` subscriptions |
| SSE | `/api/v1/GraphQL/sse` | POST subscriptions using distinct connections and `text/event-stream` |

SSE is additive. One HTTP request owns one subscription; disconnecting cancels it. GET/EventSource and multiplexed single-connection SSE reservations are not implemented. Queries/mutations continue through the existing HTTP endpoint. SSE uses `next` events containing GraphQL execution results and an empty-data `complete` event for normal completion. Validation errors appear in a `next` result followed by `complete`. An interrupted connection is not normal completion: reconnect and resubscribe.

## Local test

Start both existing Function Apps with the native hook and resolved Redis/bridge settings. Use port 7071 for GraphQL and 7072 for Stitch, with Stitch's existing `Graphql:Endpoints` pointing at the local GraphQL HTTP endpoint. The hook now installs both transports; no new app settings are needed. If `NativeWebSocketsPath` is customized, SSE uses the sibling `sse` path.

In Postman create a POST to `http://localhost:7072/api/v1/GraphQL/sse`, set `Content-Type: application/json` and `Accept: text/event-stream`, and send:

```json
{"query":"subscription { conciergeMessageReceived(conversationId: \"12345678-1234-1234-1234-123456789abc\") { messageId message response } }"}
```

Keep that response open. From a second request POST to `http://localhost:7072/api/v1/GraphQL/`:

```json
{"query":"mutation { postConciergeMessage(conversationId: \"12345678-1234-1234-1234-123456789abc\", message: \"ping\") { messageId response } }"}
```

The open SSE response should receive `event: next` with `response: "pong"` and the matching message ID. Repeat with `message: "pong"` to receive `ping`. Cancel the SSE request to release the subscription. Public Concierge operations need no bearer token; supply Authorization for protected fields. Existing GraphQL authorization remains authoritative.

## Stitch and scale-out

The SSE host handler dispatches fresh internal requests through the Functions-host middleware to the private worker bridge, using the existing bridge secret. The host invokes the isolated worker normally without an HTTP self-callback. It marks these callbacks with `Accept: text/event-stream`. Stitch captures this header and chooses its SSE remote stream; it derives HTTPS `/sse` URLs from `Graphql:Endpoints`. WebSocket callbacks retain the existing downstream WebSocket implementation. Domain schemas and Redis session routing are shared, with no direct GraphQL project reference from Stitch.

Responses are flushed after each event. Idle bridge reads emit SSE comments as keepalives. Sessions retain the existing five-minute lifetime, capacity and Redis cleanup limits. Results and incoming requests are bounded. Redis Pub/Sub does not replay missed events, and SSE event IDs/resume cursors are not supplied. Clients must handle reconnects and application-level reconciliation when delivery matters.

## DEV deployment and APIM

Deploy the GraphQL backend(s) referenced by `Graphql:Endpoints` and the Stitch Function App using the existing pipelines. Packaging already includes the host extension DLL, so no new Azure resources or hook settings are required.

`VAA.EAPI.GraphQL.Mobile.Deployment/Configure-DevSseRoute.ps1` previews two POST `/sse` operations under the existing public DEV GraphQL and Stitch HTTP APIs. Run with `-Apply` only after both backends are deployed. The script preserves existing APIs/WebSocket routes and refuses to replace a differing SSE policy. It sets the full backend path and `buffer-response="false"`. No live APIM changes are performed by building this branch.

Expected client URL after provisioning: `https://testresources.virginatlantic.com/dev/GraphQL/mobile/public/sse`. The downstream general URL is `https://testresources.virginatlantic.com/dev/GraphQL/public/sse`.

Inherited APIM policies, diagnostics, Front Door and WAF must also permit streaming. Avoid response-body transformations, buffering, body logging and caching for SSE. Inspect inherited policies before applying; the operation-level backend intentionally uses unbuffered forwarding. The existing public 403 rejection still needs diagnosis and is not fixed by changing transport.

The native Functions host startup hook remains an unsupported extension. Local Kestrel tests do not prove Azure Functions/Front Door connection duration. Validate immediate incremental delivery, idle keepalives, reconnects, scale-out, deployment interruption and both transports in DEV before treating this as production-ready.
