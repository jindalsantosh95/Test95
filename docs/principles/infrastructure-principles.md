# Infrastructure Principles
**Virgin Atlantic EAPI Platform**

---

EAPI infrastructure is Azure-first and encoded through Function host startup, shared infrastructure libraries, deployment templates, Key Vault-backed parameters, and Azure DevOps pipeline YAML. Infrastructure changes must be reviewed with the same care as application code because they define runtime security, scale, observability, and release behaviour.

## Azure Platform

Azure is the default hosting and integration platform for EAPI.

- Azure Functions are the default compute model for microservices, facades, web APIs, NDC transformation functions, and orchestration workers.
- Azure API Management is the expected public and partner API boundary.
- Azure Service Bus is the standard async messaging backbone.
- Azure Key Vault is the standard secret source.
- Application Insights and Azure Monitor are the standard observability tools.
- Azure Storage, Redis, Cosmos DB, and SQL-backed services should be used through existing platform abstractions.

## Function Host Baseline

Every new Function service should start from the shared baseline.

- Configure command-line and environment configuration providers.
- Use `ConfigureFunctionsWorkerDefaults(...)` and default middleware.
- Enable OpenAPI with `ConfigureOpenApi()`.
- Register `AddDefaultServices()` or equivalent explicit baseline services.
- Register health checks, Application Insights worker telemetry, memory cache, response compression for HTTP services, and telemetry initialisers.
- Register `IHttpConnector` and the required HTTP client factory support for outbound HTTP.

## Middleware

Middleware order and reuse matter.

- Use shared default middleware where available: function context accessor, HTTP compression, exception handling, request validation, authentication, and caching as appropriate for the host.
- Do not duplicate middleware behaviour inside individual triggers.
- Preserve exception handling middleware when refactoring startup.
- For GraphQL, respect the host pipeline for authentication, request validation, caching, operation limits, timeouts, error filtering, introspection controls, and telemetry.
- If service-specific middleware is needed, add it after the shared baseline unless the established host pattern requires otherwise.

## Configuration

Configuration must be environment-driven and secret-safe.

- Centralise environment variable names in `Constants/Environment.cs` or the project's established constants class.
- Prefer `AddEnvironmentConfiguration()` and `ConfigReader` patterns already present in the service.
- App settings and deployment parameters should reference Key Vault for secrets.
- Build artifacts should be environment-agnostic; environment differences belong in parameters, app settings, Key Vault, or pipeline variables.
- Do not hard-code environment URLs, tenant IDs, subscription keys, connection strings, or resource names in feature code.

## Deployment Templates

Infrastructure changes must live with the deployment project that owns the service.

- Update `VAA.EAPI.Deployments` or the service-specific deployment folder when app settings, Function Apps, APIM resources, Service Bus topics, Key Vault references, storage, or monitoring requirements change.
- Maintain environment parameter files for dev, sit, uat, prod, sandbox, preview, or service-specific environments where they already exist.
- Preserve slot templates and APIM templates when a service uses them.
- Do not make manual portal-only changes without capturing them in deployment source.
- Keep deployment naming aligned with the service and existing resource conventions.

## CI/CD

Pipelines are part of the product.

- Update `VAA.EAPI.Pipelines` or `VAA.EAPI.Pipelines.DigitalServices` when build, test, deployment, APIM, slot, or functional test behaviour changes.
- Prefer solution-scoped build and test commands in pipelines and local verification.
- Unit tests, integration tests, security scans, and functional tests should run at the smallest useful scope.
- Deployment gates must prevent broken or unverified artifacts from reaching higher environments.
- PR descriptions should include impacted services and commands run.

## Observability

Every service must be operable in production.

- Register Application Insights worker telemetry and telemetry initialisers consistently.
- Preserve correlation IDs and operation IDs across HTTP, GraphQL, Service Bus, timers, and storage/event store operations.
- Expose heartbeat and health endpoints for HTTP services and dependency health checks where applicable.
- Emit structured logs with technical error codes and service context.
- Avoid high-cardinality or PII-heavy telemetry dimensions.
- Production alerts must map to a runbook or operational response path.

## Resilience And Scale

Infrastructure must support retryable, scalable operations.

- Azure Functions should remain stateless except for externalised stores, queues, caches, and event stores.
- Use Redis, storage leases, or dispatcher patterns deliberately for coordination.
- Service Bus consumers must handle duplicate delivery and dead-letter scenarios.
- Long-running or downstream-dependent work should move to asynchronous processing where request timeout is a risk.
- Configure cache lifetimes, queue names, connection strings, and concurrency settings through environment configuration.

## Local Development

Local work should be fast and scoped.

- Do not build or test from the repo root unless the user explicitly asks.
- Use the solution that owns the service being changed.
- Prefer `dotnet restore <solution>.sln`, `dotnet build <solution>.sln -c Release`, and `dotnet test <solution>.sln -c Release`.
- Keep local-only configuration out of source control.
- Do not rely on production secrets or live customer data for local testing.

## AI Agent Conformance Checklist

- Did I update the owning deployment or pipeline files when runtime configuration changed?
- Does startup include default services, middleware, OpenAPI, health checks, telemetry, cache, compression, and `IHttpConnector` as applicable?
- Are secrets Key Vault-backed and out of source?
- Is observability preserved with correlation IDs and health checks?
- Did I avoid root-level build/test commands?
- Did I report the exact verification command or explain why it was skipped?

