# VAA.EAPI.GraphQL Development Guide

This guide documents how to develop safely in `VAA.EAPI.GraphQL` using the repository platform principles and coding standards.

It is intended for engineers and AI agents making resolver, host, routing, integration, and test changes in this solution.

## Purpose And Scope

Use this solution for:

- GraphQL query and mutation composition.
- REST-to-GraphQL routing and MCP tool exposure.
- Stitch gateway composition over remote GraphQL endpoints.
- Domain resolver/service behavior in `VAA.EAPI.GraphQL.Domain.*`.

Avoid placing business workflows in host entrypoints. Keep host and routing layers thin and delegate to domain/infrastructure services.

## Host Matrix

| Host | Project | Bootstrap | Main Endpoints |
| --- | --- | --- | --- |
| Primary GraphQL | `VAA.EAPI.GraphQL` | `Program.cs` -> `FunctionHost/HostBuilderInitialiser.cs` | `POST /api/v1/GraphQL`, `GET /api/v1/GraphQL/Schema`, `GET /api/heartbeat`, `Warmup` |
| REST Router | `VAA.EAPI.GraphQL.Rest` | `Program.cs` -> `FunctionHost/HostBuilderInitialiser.cs` | `GET/POST /api/v1/{queryMutationName}`, `OPTIONS /api/v1/{queryMutationName?}`, `POST /api/mcp`, `GET /api/.well-known/oauth-protected-resource/public/mcp`, `GET /api/.well-known/oauth-authorization-server`, `GET /api/heartbeat`, `Warmup` |
| Stitch Gateway | `VAA.EAPI.GraphQL.Stitch` | `Program.cs` | `GET/POST /api/v1/graphql/{**slug}`, `GET /api/heartbeat`, `Warmup` |

Notes:

- Azure Functions adds the `/api` prefix by default.
- Primary/REST hosts discover domain assemblies and execute each domain `Startup : IStartup`.
- REST host instantiates `Router` at startup to precompute route mappings and capability output.

## Domain Startup Inventory

Current domain projects exposing `Startup.cs`:

- `VAA.EAPI.GraphQL.Domain.Account`
- `VAA.EAPI.GraphQL.Domain.Book`
- `VAA.EAPI.GraphQL.Domain.Holidays`
- `VAA.EAPI.GraphQL.Domain.Loyalty`
- `VAA.EAPI.GraphQL.Domain.NDCHealthCheck`
- `VAA.EAPI.GraphQL.Domain.Operations`
- `VAA.EAPI.GraphQL.Domain.PaxOps`
- `VAA.EAPI.GraphQL.Domain.RefData`
- `VAA.EAPI.GraphQL.Domain.Retailing`
- `VAA.EAPI.GraphQL.Domain.Search`
- `VAA.EAPI.GraphQL.Domain.Security`
- `VAA.EAPI.GraphQL.Domain.Servicing`
- `VAA.EAPI.GraphQL.Domain.Trips`
- `VAA.EAPI.GraphQL.Domain.VHols`
- `VAA.EAPI.GraphQL.Domain.WebForms`

Each domain startup can contribute:

- configuration sources (`ConfigureAppConfiguration`)
- worker middleware (`ConfigureFunctionsWorkerDefaults`)
- DI registrations (`ConfigureServices`)
- GraphQL operations (`RegisterQueries`, `RegisterMutations`)
- health checks (`RegisterHealthChecks`)

For business capability and onboarding ownership routing by domain project, see `DOMAIN-OWNERSHIP-MAP.md`.

## Development Rules For This Solution

Apply these during implementation and review:

- Use `IHttpConnector` for feature-level outbound HTTP in domain/infrastructure code.
- Keep JSON contract behavior aligned with established `Newtonsoft.Json` usage.
- Keep resolver and router layers thin; delegate orchestration to domain services.
- Register all dependencies explicitly in DI; avoid service locator patterns.
- Preserve existing auth and visibility behavior (`OperationsVisibilityMode`, role checks).
- Keep external transport and partner-specific mapping below the GraphQL boundary.

## Adding Or Changing A GraphQL Operation

1. Add or update query/mutation method in the owning `VAA.EAPI.GraphQL.Domain.*` project.
2. Register operation type in domain `Startup` (`RegisterQueries`/`RegisterMutations`).
3. Keep secure/public behavior consistent with `isSecure` handling.
4. If REST router behavior is required, ensure the method signature is routable and documented via generated route metadata.
5. Add or update focused tests in the owning `*.Tests` project.
6. Update deployment/pipeline artifacts if runtime/config shape changes.
7. Update this solution documentation when introducing durable behavior or wiring changes.

## Testing And TDD Guidance

- Prefer domain/service tests over trigger-only tests for business behavior.
- Use shared xUnit infrastructure and register:
  - `IHttpClientFactory` as `VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory` for unit tests to enforce the shared HTTP pipeline and no-live-network behavior.
  - `IHttpConnector` through DI.
- Use `.mocks/*.mock` test files for deterministic downstream behavior.
- Ensure `.mocks` content is copied to output when required.
- Do not call live downstream systems from unit tests.

## Verification Commands

Run from repository root:

```pwsh
dotnet restore VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln
dotnet build VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln -c Release
dotnet test VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln -c Release
```

Stitch scope:

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

## Important Technical Notes

- Primary/REST hosts are on HotChocolate v12 packages; Stitch host uses HotChocolate v13 packages. Treat host-specific behavior and extension APIs accordingly.
- REST `WellKnownFunction` metadata currently returns placeholder values (`your-server.com`/`auth.your-server.com`) and should be reviewed before any production-facing use.
- Startup assembly discovery in primary/REST hosts is based on `VAA.EAPI.GraphQL.Domain.*.dll` in the host output directory; missing domain binaries in output means missing startup registrations at runtime.

## Review Checklist

- Are query/mutation registrations updated in the correct domain startup?
- Is outbound HTTP routed through `IHttpConnector`?
- Are serialization and error behaviors consistent with current contracts?
- Are auth/visibility semantics unchanged unless explicitly intended?
- Are unit tests deterministic and free from live network dependencies?
- Are deployment/pipeline updates included when runtime/config changed?
