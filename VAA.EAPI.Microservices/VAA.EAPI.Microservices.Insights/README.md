# VAA.EAPI.Microservices.Insights

## Purpose
`VAA.EAPI.Microservices.Insights` is the **platform operations insight service** for EAPI.

It is an Azure Functions isolated-worker microservice that gives engineering/support teams a single place to:
- check platform health,
- inspect API catalogue entries,
- query operational metrics from Azure resources,
- inspect Azure subscriptions/resource groups/sites/app settings,
- monitor and compare Function App configuration drift,
- compile and retrieve Azure DevOps deployment status,
- retrieve GitHub pull requests merged into each repository's identified default branch across all repositories visible to a supplied PAT token for a supplied GitHub owner.

This service is **operations-facing** (control-plane/diagnostics) rather than a customer-facing business-domain API.

## Runtime and technical profile
- **Framework:** .NET 10 (`net10.0`)
- **Functions model:** Azure Functions v4, isolated worker
- **Hosting entry point:** `Program.cs`
- **OpenAPI:** enabled with `ConfigureOpenApi()`
- **Dependency injection:** configured centrally in `Program.cs`
- **Messaging:** Azure Service Bus trigger for ADO events (excluded during `DEBUG` builds)
- **Caching:** distributed Redis cache + in-memory cache registrations

## What this service owns
### 1) Health and catalogue visibility
- Heartbeat endpoint for liveness/dependency checks.
- Catalogue endpoint to discover API endpoints for a given environment.

### 2) Azure infrastructure metrics aggregation
Exposes metrics-oriented endpoints for:
- Redis cache,
- Service Bus,
- deployment metadata,
- Cosmos DB,
- service plans,
- Application Insights (with caller-supplied `AppId`/`ApiKey`).

### 3) Azure resource discovery utilities
Provides read APIs over:
- subscriptions,
- resource groups (with optional `projectCode` filter),
- app service/function sites,
- Service Bus namespaces with topics, subscriptions, and active/dead-letter queue depths,
- app settings for a specific site.

### 4) Function app settings drift monitoring
Supports app settings snapshot/compare plus alert lookback for configuration differences, including:
- ad hoc compare,
- explicit snapshot operations,
- full compare execution,
- timer-driven scheduled full compare.

### 5) ADO deployment status compilation
Supports both pull and push paths:
- HTTP endpoints to retrieve/compile/reset deployment status, including targeted single-pipeline compilation,
- timer-based daily compilation,
- Service Bus event processing for near-real-time updates.

## Solution structure (high level)
- `Application/Query` – request/handler flows used by metrics and catalogue APIs.
- `Application/Providers` – provider/process logic (catalogue, metrics, ADO processing).
- `Application/AppSettingsMonitor` – settings snapshot/compare/alert workflows.
- `Application/GitHub` – orchestration handlers for GitHub query use-cases.
- `Infrastructure/Azure` – Azure Resource Manager integration and models.
- `Infrastructure/GitHub` – GitHub REST API integration and DTO mappings.
- `Triggers/Http` – HTTP endpoints.
- `Triggers/Timer` – scheduled jobs.
- `Triggers/ServiceBus` – ADO pipeline event processing.
- `Constants/` – environment keys and app constants.

## HTTP endpoints
> All endpoints are Function routes under `/api/*`.

### Health and catalogue
- `GET /api/heartbeat`
- `GET /api/catalogue?environment={environment}`
- Warmup trigger: Function warmup hook (`Warmup`)

### Metrics
- `GET /api/rediscachemetrics`
- `GET /api/servicebusmetrics`
- `POST /api/deploymentmetrics`
- `POST /api/cosmosmetrics`
- `GET /api/serviceplanmetrics`
- `POST /api/appinsightsmetrics`

### Azure DevOps deployment status
- `GET /api/deploymentstatus`
- `POST /api/deploymentstatus/compile`
- `POST /api/deploymentstatus/compile/pipeline`
  - Request body: `{ "pipelineId": 123, "pipelineName": "Pipeline name" }`
  - Compiles deployment status for the supplied pipeline.
- `POST /api/deploymentstatus/reset`

### GitHub insights
- `POST /api/github/pullrequests/merged`
  - Request body must include `personalAccessToken`, `owner`, `startDate`, and `endDate` (`yyyy-MM-dd`).
  - `startDate`/`endDate` are inclusive calendar dates; the service evaluates merges in UTC and includes PRs merged from `startDate 00:00:00Z` through `endDate 23:59:59Z`.
  - For each repository visible to the PAT and matching `{owner}/*`, the service resolves the repository's default branch from GitHub metadata (falling back to `main` when the field is missing).
  - Merged PR retrieval is branch-aware (`base=<defaultBranch>`), only includes PRs with `merged_at` in range, and returns each repository even when no PRs are found.
  - Response items include `repository`, `defaultBranch`, `pullRequests`, and `error` (populated per repository when upstream GitHub API calls fail).
  - Processing is parallelized (up to 10 repositories at a time) to improve response times for large owners.

### Azure resource discovery
- `GET /api/azure/subscriptions`
- `GET /api/azure/subscriptions/{subscriptionId}/resourcegroups?projectCode={optional}`
- `GET /api/azure/subscriptions/{subscriptionId}/resourcegroups/{resourceGroup}/sites`
- `GET /api/azure/subscriptions/{subscriptionId}/resourcegroups/{resourceGroup}/servicebus`
- `GET /api/azure/subscriptions/{subscriptionId}/resourcegroups/{resourceGroup}/sites/{appName}/settings`

### App settings monitor
- `GET /api/appSettings/diffalerts?timeframe={hh:mm:ss}`
- `POST /api/appSettings/fullcompare`
- `POST /api/appSettings/keyvaultresolverstatus/fullcheck`
  - Scans Function App settings backed by Key Vault and emits a warning for references whose resolver status is not `Resolved`.
  - Azure App Service `429 Too Many Requests` responses are retried up to three times. The service honours `Retry-After` when supplied; otherwise it uses bounded exponential backoff with jitter. Each retry delay is capped at 30 seconds.
- `GET /api/appSettings/compare?subscriptionId=...&resourceGroupName=...&appName=...`
- `POST /api/appSettings/snapshot?subscriptionId=...&resourceGroupName=...&appName=...`

## Background jobs and event processing
- **Timer:** `AppSettingsMonitorFullCompareTimer`
  - CRON: `0 0 * * * *` (hourly)
  - Executes the full app-settings compare workflow and Key Vault resolver-status scan.
- **Timer:** `CompileADODeploymentStatus`
  - CRON: `0 0 4 * * *` (daily at 04:00 UTC)
  - Compiles ADO deployment status cache.
- **Service Bus:** `ADOServiceBusTrigger`
  - Topic: `adopipelineevents`
  - Subscription: `%ADOPipelineSubscription%`
  - Filters/processes qualifying main-branch pipeline events.

## Key dependencies registered in startup
- `IHttpConnector` via `HttpConnector`
- `ICatalogueProvider`, `IMetricsProvider`
- `IADODeploymentCompiler`, `IADOPipelineProcessor`
- `IAppSettingsMonitor`, `IAzureResources`
- `IOAuthTokenProvider`
- Redis cache + distributed cache abstraction + memory cache
- MediatR registrations for query handlers and pipeline logging behavior
- `ServiceBusClient` configured with WebSockets transport

## Configuration (environment variables)
Configuration is sourced from command line + environment configuration providers.

Primary key groups used by this service include:
- **APIM / catalogue / health**
  - `ApimSubscriptionApiClientId`
  - `ApimSubscriptionApiClientSecret`
  - `ApimSubscriptionApiResource`
  - `ApimSubScriptionId`
  - `ApimApiVersion`
  - `ApimHostName`
  - `ApimEnvironment`
  - `HealthCheckSubscriptionKey`
  - `ApimHealthCheckAuthClientId`
  - `ApimHealthCheckAuthClientSecret`
  - `HealthCheckAuthClientId`
  - `HealthCheckAuthClientSecret`
- **ADO / deployment status**
  - `DevOpsPAT`
  - `DevOpsBaseUrl`
  - `DevOpsProject`
  - `ADOPipelineServiceBus`
  - `ADOPipelineSubscription`
- **Metrics**
  - `MetricsSubscriptionClientId`
  - `MetricsSubscriptionClientSecret`
  - `MetricsSubscriptionId`
  - `MetricsRedisApiVersion`
  - `MetricsServiceBusApiVersion`
  - `MetricsInsightsVersion`
  - `RedisConnectionString`
- **App settings monitor**
  - `AppSettingsMonitorDefaultSubscriptions`
  - `AppSettingsMonitorSnapshotStorageConnectionString`
  - `AppSettingsMonitorSnapshotStorageContainerName`
  - `AppSettingsMonitorAppInsightsAppId`
  - `AppSettingsMonitorAppInsightsAPIKey`

## Local development
From the `VAA.EAPI.Microservices` solution folder:

```bash
dotnet restore VAA.EAPI.Microservices.sln
dotnet build VAA.EAPI.Microservices.sln -c Release
```

Run Insights-focused tests (if available in your local setup):

```bash
dotnet test VAA.EAPI.Microservices.sln -c Release --filter Insights
```

Project-only build:

```bash
dotnet build VAA.EAPI.Microservices.Insights/VAA.EAPI.Microservices.Insights.csproj -c Release
```

## Security and operational considerations
- Most endpoints are anonymous function routes; deploy behind APIM/network controls and appropriate auth policy.
- Returned data can include operationally sensitive infrastructure/configuration metadata.
- Keep all secrets (PATs, API keys, connection strings, client secrets) in approved secret stores.
- The service currently uses its own identity/token provider for ARM access in Azure/app-settings operations.

## Related artifacts
- `VAA.EAPI.Microservices.Insights.Tests` – tests for Insights components.
- `VAA.EAPI.Microservices.Insights.Bruno` – Bruno collection for endpoint testing.
