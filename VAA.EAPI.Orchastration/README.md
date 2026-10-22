# VAA.EAPI.Orchastration

Orchestration Azure Functions for cross-domain workflows, currently packaged in `VAA.EAPI.Orchestration.sln`.

## Projects in this solution

- `VAA.EAPI.Orchestration.Notifications`
- `VAA.EAPI.Orchestration.RetailTherapy`
- `VAA.EAPI.Orchestration.Notifications.Tests`
- `VAA.EAPI.Orchestration.RetailTherapy.Tests`
- deployment projects for both function apps

## Runtime and implementation snapshot

Both function apps target `.NET 10` and use Azure Functions isolated worker with OpenAPI and default middleware.

Both hosts inherit configurable Application Insights/Datadog logging and W3C correlation through `AddDefaultServices()`. Deployment configuration is supplied through one secure `DataDog` object; select `AppInsights`, `Datadog`, or `Both` with `LoggingProvider`, and source `ApiKey` from the common Key Vault secret.

Common startup conventions currently implemented in both `Program.cs` files:

- `AddCommandLine(args)` + `AddEnvironmentConfiguration()`
- `ConfigureFunctionsWorkerDefaults(worker => worker.AddDefaultMiddleware())`
- `ConfigureOpenApi()`
- `services.AddDefaultServices()`
- `services.AddSingleton<IHttpConnector, HttpConnector>()`

Service-specific registrations:

- `RetailTherapy`: AWS S3 + crew/flight services.
- `Notifications`: Redis cache, Service Bus, delayed check-in and Starlink dispatchers, Starlink eligibility checks, and event-store command handlers.

## Local validation (from this folder)

```bash
dotnet restore VAA.EAPI.Orchestration.sln
dotnet build VAA.EAPI.Orchestration.sln -c Release
dotnet test VAA.EAPI.Orchestration.Notifications.Tests/VAA.EAPI.Orchestration.Notifications.Tests.csproj -c Release
dotnet test VAA.EAPI.Orchestration.RetailTherapy.Tests/VAA.EAPI.Orchestration.RetailTherapy.Tests.csproj -c Release
```

For harness verification and hook setup, see:

- [`../docs/readme.md`](../docs/readme.md)
- [`../docs/harness-engineering.md`](../docs/harness-engineering.md)
- [`../docs/knowledge-repo-guide.md`](../docs/knowledge-repo-guide.md)

Codex lifecycle hooks for agent guardrails are configured at:

- [`../.codex/hooks.json`](../.codex/hooks.json)
