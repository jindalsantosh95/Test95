# VAA.EAPI.GraphQL

GraphQL channel and gateway solution for EAPI, including:

- the primary GraphQL Functions host (`VAA.EAPI.GraphQL`)
- the REST-to-GraphQL gateway (`VAA.EAPI.GraphQL.Rest`)
- the stitched GraphQL gateway (`VAA.EAPI.GraphQL.Stitch`)
- domain resolver libraries and supporting common/infrastructure libraries
- solution-scoped deployment and pipeline assets

This README is the solution-level entry point for engineers and AI agents working in `/VAA.EAPI.GraphQL`.

## Development Guide

### Outbound HTTP streaming

The GraphQL `IHttpConnector` raw response overloads use `ResponseHeadersRead`, matching
the microservice connector. Successful raw responses return without consuming their
bodies. The default HTTP diagnostics handler counts bytes as content is consumed or
disposed rather than downloading it for timing. Partial consumption does not fabricate
a total Content-Length. Typed `APIResponse<T>` methods still consume and deserialize
the complete JSON body; overloads with `noWait` retain existing HTTP error mapping and
read error bodies. The raw overloads without `noWait` leave status handling to callers.

Streaming callers must dispose the response/stream, check status codes, pass cancellation
to body reads and enforce a body lifetime/idle deadline: HttpClient's request timeout
only covers receipt of headers in this mode. Do not use typed JSON methods for SSE.
Use a non-cached route; caching or mock response capture can still consume a body.
This provides outbound streaming support, not an SSE parser, a streaming Function
response endpoint, or deployed end-to-end Concierge streaming acceptance.

For implementation workflow, host/domain inventory, testing conventions, and verification commands, see:

- `DEVELOPMENT-GUIDE.md`
- `DOMAIN-OWNERSHIP-MAP.md` (domain-to-capability-to-team onboarding map)

## Scope And Ownership

Use this solution when the change belongs to GraphQL channel composition, resolver behavior, GraphQL-to-downstream integration, or REST gateway routing to GraphQL operations.

Keep business workflows in domain services/providers and keep resolver/router entrypoints thin.

## Solution Map

- `VAA.EAPI.GraphQL.sln`
  - Core GraphQL host: `VAA.EAPI.GraphQL/`
  - REST gateway host: `VAA.EAPI.GraphQL.Rest/`
  - Domain libraries: `VAA.EAPI.GraphQL.Domain.*`
  - Shared libraries: `VAA.EAPI.GraphQL.Common/`, `VAA.EAPI.GraphQL.Infrastructure/`, `VAA.EAPI.GraphQL.Core.*`
  - Tests: `VAA.EAPI.GraphQL.*.Tests`, `VAA.EAPI.GraphQL.Framework.Tests`
  - Deployment: `VAA.EAPI.GraphQL.Deployment/`
  - Pipelines: `VAA.EAPI.GraphQL.Pipelines/` and linked pipeline assets in `../VAA.EAPI.Pipelines*`
- `VAA.EAPI.GraphQL.Stitch.sln`
  - Stitch host: `VAA.EAPI.GraphQL.Stitch/`
  - Deployment: `VAA.EAPI.GraphQL.Stitch.Deployment/`, `VAA.EAPI.GraphQL.Mobile.Deployment/`

## Runtime Hosts And Entry Points

### Primary GraphQL Host (`VAA.EAPI.GraphQL`)

- Host bootstrap: `Program.cs` + `FunctionHost/HostBuilderInitialiser.cs`
- GraphQL endpoints:
  - `POST /api/v1/GraphQL`
  - `GET /api/v1/GraphQL/Schema`
- Operational endpoints:
  - `GET /api/heartbeat`
  - `Warmup` trigger

The host discovers `VAA.EAPI.GraphQL.Domain.*` assemblies, loads each domain `Startup : IStartup`, and composes query/mutation registration through `RegisterQueries` and `RegisterMutations`.

### REST Gateway (`VAA.EAPI.GraphQL.Rest`)

- Host bootstrap: `Program.cs` + `FunctionHost/HostBuilderInitialiser.cs`
- Routing endpoints:
  - `GET /api/v1/{queryMutationName}`
  - `POST /api/v1/{queryMutationName}`
  - `OPTIONS /api/v1/{queryMutationName?}` (markdown capability docs)
  - `POST /api/mcp` (Model Context Protocol handler)
- Metadata endpoints:
  - `GET /api/.well-known/oauth-protected-resource/public/mcp`
  - `GET /api/.well-known/oauth-authorization-server`
- Operational endpoints:
  - `GET /api/heartbeat`
  - `Warmup` trigger

The `Router` builds route mappings from registered resolver/query/mutation types and returns generated markdown docs for OPTIONS calls.

### Stitch Host (`VAA.EAPI.GraphQL.Stitch`)

- Host bootstrap: `Program.cs`
- Stitched GraphQL endpoint:
  - `GET|POST /api/v1/graphql/{**slug}`
- Operational endpoints:
  - `GET /api/heartbeat`
  - `Warmup` trigger

Stitch host composes remote schemas from configured endpoints (`Graphql:Endpoints`) and applies shared middleware/interceptors.

## Domain Extension Pattern

Domain projects follow `Startup : IStartup` and provide:

- `ConfigureAppConfiguration`
- `ConfigureFunctionsWorkerDefaults`
- `ConfigureServices`
- `RegisterQueries`
- `RegisterMutations`
- `RegisterHealthChecks`

`RegisterQueries` and `RegisterMutations` receive `isSecure` and typically gate operations by visibility mode while allowing debugger-driven local development paths.

## Integration And Serialization Conventions

- Outbound feature HTTP should go through the shared connector (`IHttpConnector`), not ad hoc `HttpClient` usage in resolver/service code.
- JSON serialization/deserialization for service contracts should align with established `Newtonsoft.Json` usage in shared libraries and current contract behavior.
- Keep partner/external transport details in provider/infrastructure layers, not in GraphQL boundary models.

## Local Verification Commands

Run from repository root with solution scope:

```pwsh
dotnet restore VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln
dotnet build VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln -c Release
dotnet test VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln -c Release
```

For stitch scope:

```pwsh
dotnet restore VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Stitch.sln
dotnet build VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Stitch.sln -c Release
dotnet test VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Stitch.sln -c Release
```

Harness checks:

```pwsh
scripts/verify.ps1 -SolutionPath VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln -SkipBuild
scripts/verify.ps1 -SolutionPath VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Stitch.sln -SkipBuild
```

## Testing Guidance (GraphQL Scope)

- Prefer focused tests in the owning domain test project.
- Keep mock files under `.mocks`/test-data conventions used by the project.
- Use shared xUnit infrastructure (`VAA.EAPI.GraphQL.Core.Xunit`) and DI-registered HTTP factories/connectors where applicable.
- Avoid live downstream calls in unit tests.

## Bruno Collection

Open `VAA.EAPI.GraphQL.Bruno/` in Bruno for requests generated from the Dev full-public GraphQL schema. The collection groups queries and mutations separately and includes Dev, SIT, UAT, and Production environments. Run its `generate.ps1` script to refresh operation signatures from Dev introspection.

For a collection scoped only to the WebForms domain, open `VAA.EAPI.GraphQL.Domain.WebForms.Bruno/`. It contains the public refund query and mutation plus the secure payment-update mutation, with local, Dev, SIT, UAT, and Production environments.

## Deployment And Pipeline References

All four GraphQL slot templates preserve existing sticky settings when adding
`NativeWebSocketsRedisPrefix`. Their nested templates use a literal empty-array
variable for missing or null `appSettingNames`; avoid `json('[]')` here because
build 726475 failed during ARM evaluation of that fallback. Run
`pwsh -File scripts/test-graphql-native-deployment.ps1` from the repository root
to check this contract across the standard, digital, Mobile, and Stitch templates.

- Deployments:
  - `VAA.EAPI.GraphQL.Deployment/`
  - `VAA.EAPI.GraphQL.Stitch.Deployment/`
  - `VAA.EAPI.GraphQL.Mobile.Deployment/`
- Solution-scoped pipeline templates/variables:
  - `VAA.EAPI.GraphQL.Pipelines/functions/`
  - `VAA.EAPI.GraphQL.Pipelines/variables/`
- Additional linked pipeline definitions are referenced in the `.sln` from `../VAA.EAPI.Pipelines*`.

## Custom-Event Telemetry Destinations

Existing GraphQL business custom events sent through `ITelemetryService.TrackTelemetry` follow `LoggingProvider`: `AppInsights` preserves the existing Application Insights event projection, `Datadog` sends only a direct Datadog event, and `Both` sends independent projections to both destinations. Invalid or missing values use the established `AppInsights` default.

The Datadog projection never forwards dynamic `TelemetryEvent.Data`. It contains only the event name, event ID, source, timestamp, fixed `Information` severity, and safe EAPI/W3C correlation context. The existing `DataDog` deployment settings remain the configuration contract; `DatadogApiKey` must remain Key Vault-backed. Datadog delivery accepts only the documented regional HTTPS Logs Intake endpoint with the exact `/api/v2/logs` path, queues delivery without blocking callers, and exports bounded delivery metrics through the existing Application Insights pipeline for safe operational monitoring.

## Change Expectations

When changing behavior in this solution:

- update domain startup registrations when adding/removing operations
- keep route/contract docs and README notes current for new public endpoints
- update deployment/pipeline artifacts when runtime wiring or config surface changes
- keep changes in the smallest owning project and avoid broad cross-domain refactors

## Young Flyer Configuration

The `accountCreateYoungFlyerAccount` mutation validates a Young Flyer's date of birth against the `YoungFlyerAgeLimit` application setting. It is an integer and defaults to `18` when not configured; set it to a different value, such as `16` or `21`, without changing code. The mutation rejects requests when the authenticated parent's account is not active. Use `accountManagedYoungFlyerAccounts` to retrieve the authenticated parent or guardian's managed Young Flyer accounts.

`EnableUnder18StandardEnrolmentBlock` controls whether standard and Entra enrolment reject applicants under 18. It is independent of the configurable `YoungFlyerAgeLimit`, defaults to `false` when absent, and the deployment parameters set it to `false` until the Young Flyer route is enabled.

## Related Platform Docs

- `docs/principles/architecture-principles.md`
- `docs/principles/integration-principles.md`
- `docs/principles/infrastructure-principles.md`
- `docs/principles/security-principles.md`
- `docs/standards/coding-standards.md`
- `docs/design-patterns/readme.md` (see the GraphQL composition patterns section)
