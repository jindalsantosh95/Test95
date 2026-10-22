# GraphQL Domain Startup Pack

**Pattern group:** Platform Runtime

**Status:** Recommended

**Last reviewed:** 2026-09-30

**Owner:** EAPI Technical Leads

## Intent

Allow each GraphQL domain package to register its own services, queries, mutations, configuration, middleware, and health checks while the GraphQL host composes them into one platform API.

This keeps domain ownership local while preserving a consistent host pipeline, schema registration flow, and operational baseline.

## Use When

- Adding a GraphQL domain capability.
- Registering new query or mutation types.
- Adding domain-specific services or health checks to the GraphQL host.

## Do Not Use When

- The capability belongs in a microservice rather than GraphQL orchestration.
- A resolver can reuse an existing domain service without new startup wiring.
- The change would bypass the domain package and register implementation details directly in the host.

## Pattern Shape

1. Implement or update a domain `Startup` class that implements `IStartup`.
2. Register domain services in `ConfigureServices`.
3. Register query and mutation types in `RegisterQueries` and `RegisterMutations`.
   Subscription-capable domains can also implement the optional `IStartup.RegisterSubscriptions` callback. Its default implementation is a no-op for existing domains.
4. Let the host discover domain startup classes and compose them.

## Implementation Guidance

- Keep GraphQL domain registrations in the owning `VAA.EAPI.GraphQL.Domain.*` project.
- The primary GraphQL host invokes subscription callbacks through its `ConfigurationActions` and adds the `Subscription` root only when registrations are added, following its mutation-registration convention. Register event providers in the GraphQL host, using its schema-registration extension called by HostBuilderInitialiser. Domain `ConfigureServices` registers domain services only. This schema wiring does not itself add streaming support to the Functions transport; the REST host continues to register queries and mutations only.
- Register only the types that should be exposed in the current visibility mode.
- Keep resolvers thin and call domain services for orchestration.
- Add health checks where a domain introduces material downstream dependencies.

## Code References

- [HostBuilderInitialiser.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL/FunctionHost/HostBuilderInitialiser.cs:85): discovers domain `Startup` implementations.
- [IStartup.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Infrastructure/FunctionHost/IStartup.cs:8): contract for domain startup packs.
- [Startup.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Domain.Search/Startup.cs:35): Search domain service and query registration.

## Related Principles And Standards

- [Architecture Principles](../principles/architecture-principles.md)
- [Integration Principles](../principles/integration-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- Domain services are registered in the owning domain startup.
- Queries and mutations are registered through the domain startup methods.
- Secure/insecure visibility rules are respected.
- Resolver code remains thin and delegates to domain services.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-09-30 | Documented optional subscription startup callbacks and schema-versus-transport scope. | EAPI GraphQL |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |

Concierge uses only the existing Azure Functions SDK hosting path. The earlier alternate ASP.NET Core hosts and forwarding tests were removed: engine-level subscription registration does not prove streaming through Functions or APIM. See [transport POC history](../exec-plans/completed/concierge-subscription-transport-poc.md).
