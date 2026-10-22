# EAPI Function Host Baseline

**Pattern group:** Platform Runtime

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Provide a consistent Azure Functions host shape for EAPI services so configuration, middleware, OpenAPI, default services, telemetry, health checks, and shared connectors are wired in the same way across the platform.

This pattern makes new services predictable for engineers and AI agents, and reduces the chance that a service starts without the EAPI platform defaults.

## Use When

- Creating or materially updating an Azure Functions based EAPI service.
- Adding common middleware, connector, telemetry, configuration, or health-check wiring.
- Reviewing service startup for consistency with the platform baseline.

## Do Not Use When

- The project is not an Azure Functions app.
- A specialised host already exists and has an approved platform reason to diverge.
- The change is limited to a domain handler and does not touch startup.

## Pattern Shape

1. Build the host with `HostBuilder`.
2. Add command-line and environment-backed configuration.
3. Configure Functions worker defaults and EAPI default middleware.
4. Configure OpenAPI for HTTP services.
5. Register `AddDefaultServices()`, `AddHttpClient()`, `IHttpConnector`, telemetry, health checks, and service dependencies.

## Implementation Guidance

- Keep host wiring in `Program.cs` or the established host initialiser.
- Prefer `AddDefaultServices()` before adding service-specific registrations.
- Register `IHttpConnector` and `HttpConnector` where feature code makes outbound HTTP calls.
- Choose service lifetimes deliberately and avoid static mutable application state.
- Keep environment key names in constants and values in configuration.

## Code References

- [Program.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Microservices.CRM.Refunds/Program.cs:23): service-local host using config, default middleware, OpenAPI, default services, `IHttpConnector`, and handlers.
- [HostBuilderInitialiser.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Infrastructure/FunctionHost/HostBuilderInitialiser.cs:68): shared microservice host initialiser applying EAPI defaults.
- [HostBuilderInitialiser.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL/FunctionHost/HostBuilderInitialiser.cs:110): GraphQL host applying shared platform services and middleware.

## Related Principles And Standards

- [Architecture Principles](../principles/architecture-principles.md)
- [Infrastructure Principles](../principles/infrastructure-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- Startup includes EAPI configuration, middleware, OpenAPI, telemetry, and health-check expectations.
- `IHttpConnector` is registered where outbound HTTP is needed.
- Service dependencies are registered explicitly and in the owning project.
- No secrets or environment-specific values are hardcoded.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
