# Concierge GraphQL Subscription Delivery

Assess conversation-scoped Concierge subscription delivery through the existing GraphQL and Stitch Azure Functions hosts and APIM.

> **Minimum review:** Read the flow name and description, **Why this flow exists**, **Sequence (detailed)**, and **Open points to finalize**. These sections establish the purpose, intended interactions, and outstanding questions; review the remaining sections as relevant to your role.

## Why this flow exists

The team needs to establish whether Concierge subscriptions work through the actual deployment path. Existing Azure Function Apps and their SDK host must be retained. Schema registration and successful mutations alone do not prove streaming.

## Sequence (detailed)

Current HTTP flow is Client -> APIM -> VAA.EAPI.GraphQL.Stitch (Functions SDK host) -> VAA.EAPI.GraphQL (Functions SDK host), followed by the normal buffered response along the reverse path.

The implemented experimental subscription sequence is:

1. The client upgrades `/api/v1/GraphQL/ws` using `graphql-transport-ws`. In Azure, APIM must forward this upgrade to the existing Stitch Function App.
2. Stitch host middleware connects to `NativeWebSocketsTarget` on the GraphQL Function App and relays frames and authentication/tracing headers.
3. GraphQL host middleware forwards initialization and subscription operations through a fresh protected internal request in the Functions-host middleware pipeline, which invokes its isolated worker normally. Existing GraphQL authentication and authorization apply; anonymous operations remain possible where permitted.
4. The worker registers the Concierge subscription using its existing executor and in-memory provider. The bridge returns `connection_ack` and delivers results as `next` frames through Stitch.
5. A separate HTTP `postConciergeMessage` mutation publishes the conversation-scoped event. The waiting subscriber receives the correlated pong response.
6. Client completion or disconnect cancels and removes the subscription; the primary bridge acknowledges client close frames.

Local Core Tools testing has exercised subscription plus HTTP mutation and pong delivery. In-process transport tests cover proxy forwarding, cancellation and caller behavior, but do not substitute for deployed Azure/APIM acceptance.

## Open points to finalize

- Verify startup-hook compatibility and internal bridge dispatch on the deployed Windows Functions host.
- Confirm APIM WebSocket routing and the enforced single-instance/single-worker limits in the target environment.
- Prove the chosen path through the actual SDK host and APIM before claiming subscription delivery.

## UX contract

| Field | Value |
| --- | --- |
| Supplied UX | `None` |
| Implementation authority | `Not applicable` |
| Flow and behaviour authority | `Sequence (detailed)` |
| Screen/state mapping | Not applicable; API test clients only. |

## High-level behavior

1. Start GraphQL and Stitch through the existing Azure Functions SDK host with their normal configuration and authentication.
2. Concierge is discovered as a domain class library. GraphQL owns schema and subscription-provider registration through `builder.RegisterDomainSchema(...)`.
3. Queries and posting mutations use the existing HTTP gateway. Stitch communicates with the configured GraphQL instance over HTTP, without a project or assembly reference.
4. An experimental startup hook adds WebSocket middleware inside the existing Functions host. Stitch relays `graphql-transport-ws` frames to GraphQL, whose same-instance HTTP bridge connects to the worker subscription executor. Azure/APIM acceptance remains pending.

## Integration points

| Component | Responsibility |
| --- | --- |
| APIM | Existing HTTP exposure plus a separately configured WebSocket API route to Stitch; deployed upgrade and subscription delivery still require acceptance testing. |
| `VAA.EAPI.GraphQL.Stitch` | Azure Functions gateway with configured remote HTTP schemas and a native WebSocket relay to the configured GraphQL instance. |
| `VAA.EAPI.GraphQL` | Azure Functions gateway, domain registration and same-instance subscription bridge between host middleware and the isolated worker. |
| `VAA.EAPI.GraphQL.Domain.Concierge` | Domain class library containing dummy reply operations; not a separately hosted service. |

## Agent delivery constraints

- Target solution/project: `VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln` and `VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Stitch/VAA.EAPI.GraphQL.Stitch.csproj`.
- Existing Azure Function Apps and SDK startup are mandatory. Do not add a WebApplication/Kestrel alternative or a local-only mode to bypass the runtime.
- Preserve the normal authentication, headers, configuration and deployment pattern.
- Domain service and resolver registration stay in Concierge; event-provider selection stays in the GraphQL host extension.
- Exact operations: `conciergePing`, `conciergePong`, `postConciergeMessage`, `conciergeMessageReceived`.
- Read Concierge `Startup.cs`, the resolver classes, `Services/ConciergeMessageService.cs`, primary `FunctionHost/DomainSchemaRegistration.cs`, `Gateway.cs`, and Stitch `GraphQLFunction.cs`.
- Use the existing `Graphql:Endpoints` HTTP setting and normal Functions routes. Do not reuse the removed 5098/5099 hosts or `/subscriptions/Concierge` WebSocket route.
- The agreed experiment uses `graphql-transport-ws` on `/api/v1/GraphQL/ws`, with Stitch forwarding to `NativeWebSocketsTarget`. It needs no Web PubSub resource or replacement application host. Startup-hook integration is experimental, not an officially supported Functions WebSocket extension.
- Harness commands are listed below; no secrets, tokens or customer data belong in fixtures or documentation.

## Key non-functional considerations

- **Idempotency**: each accepted mutation generates a fresh message; no deduplication is provided.
- **Correlation**: mutation and engine event share a message ID and conversation ID.
- **Retry behavior**: in-memory events have no replay, durability or cross-instance delivery.
- **No-op path**: read-only queries do not publish events; unrelated conversations remain isolated in engine tests.
- **Authorization**: secure schema visibility is not a replacement for the Functions authentication pipeline or per-conversation authorization.

## Failure paths to capture in implementation

- Reject empty UUIDs, blank messages and messages longer than 500 characters with GraphQL validation code `400`.
- Do not treat a visible Subscription schema as confirmation that a remote subscription can run.
- Failed subscription attempts must not be silently redirected into an alternative host.
- Native sessions expire after five minutes. Require one instance and one worker process; in-memory events have no replay or cross-instance routing. Public Stitch native forwarding remains disabled to avoid exposing the Secure target schema.

## Expected harness

- `VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Domain.Concierge.Tests/ConciergeTests.cs`: dummy replies, validation and in-process event contracts.
- `VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Domain.Concierge.Tests/StartupTests.cs`: host-owned schema composition and visibility.
- `VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Tests/VAA.EAPI.GraphQL.Tests.csproj`: primary host regressions.
- `VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Domain.Concierge.Tests/NativeTransportTests.cs`: real WebSocket bridge/proxy delivery, tracing headers, close handshake and subscription cleanup.
- `VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Domain.Concierge.Tests/NativeGraphQLBridgeTests.cs` and `ConciergeCallerIsolationTests.cs`: worker boundary and caller isolation.
- Pending: deployed end-to-end subscription acceptance through the Azure Functions SDK host and APIM, including authentication, repeat replies and disconnects.

```powershell
$run = "TestResults/concierge-functions-$(Get-Date -Format yyyyMMdd-HHmmss)"
dotnet test VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Domain.Concierge.Tests/VAA.EAPI.GraphQL.Domain.Concierge.Tests.csproj -c Release --results-directory $run --logger 'trx;LogFileName=concierge.trx' --collect:'XPlat Code Coverage'
scripts/analyse-test-run-results.ps1 -TrxPath "$run/concierge.trx" -SuiteName Concierge -EnvironmentName local
scripts/verify.ps1 -SolutionPath VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln -SkipBuild
```

## Agent Loop Goal

| Field | Value |
| --- | --- |
| Goal | Deliver correlated ping/pong subscription responses using the existing Azure Functions SDK hosting path, without an alternate application host. |
| Status | `Draft` |
| Delivery surface | `Backend` |
| Owner | EAPI GraphQL maintainers |
| Review cadence | On transport, hosting, contract or provider changes |
| Last reviewed | 2026-10-06 |
| Harness Sensor | `VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Domain.Concierge.Tests/ConciergeTests.cs` |
| Promotion verification | Full |
| Acceptance evidence | Domain, bridge and proxy tests exist, plus a local Core Tools subscription/mutation/pong smoke test. Azure Functions deployment and APIM acceptance remain pending. |
| Done when | An agreed transport works through the existing Functions SDK hosts and APIM, with authenticated delivery, conversation isolation, repeated replies and executable end-to-end evidence. |

## Associated delivery pull requests

| Surface | Repository | Base branch | Head branch | PR | State | Merged at UTC |
| --- | --- | --- | --- | --- | --- | --- |
| Backend | VAAD1ST/eapi | main | codex/concierge-subscription-poc | [#2298](https://github.com/VAAD1ST/eapi/pull/2298) | Draft | Not merged |

Tracking issue: [#1773](https://github.com/VAAD1ST/concierge-api-vaa/issues/1773).

## Agent Loop Closure Evidence

Partial evidence only: native host middleware, same-instance worker bridge and Stitch relay are implemented, with automated transport tests and a local Core Tools ping/pong smoke test. Earlier alternate-host results remain withdrawn as evidence for Functions support. The goal stays Draft until deployed Azure/APIM acceptance is recorded. See the [README](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Domain.Concierge/README.md) and [historical transport plan](../exec-plans/completed/concierge-subscription-transport-poc.md).
