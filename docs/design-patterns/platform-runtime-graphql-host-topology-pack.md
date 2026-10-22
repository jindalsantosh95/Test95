# GraphQL Host Topology Pack

**Pattern group:** Platform Runtime

**Status:** Recommended

**Last reviewed:** 2026-06-25

**Owner:** EAPI Technical Leads

## Intent

Keep GraphQL runtime architecture consistent across the three host surfaces:

- primary GraphQL host (`VAA.EAPI.GraphQL`)
- REST router host (`VAA.EAPI.GraphQL.Rest`)
- stitched gateway host (`VAA.EAPI.GraphQL.Stitch`)

This pattern preserves one capability model while exposing different transport surfaces for channel and tooling needs.

## Use When

- Adding or modifying GraphQL query/mutation capabilities.
- Exposing GraphQL capabilities through REST router or MCP.
- Changing host startup, middleware, or registration behavior.

## Do Not Use When

- A capability belongs in a domain microservice instead of GraphQL orchestration.
- A change only affects one domain provider implementation and does not alter host composition.
- A transport endpoint is added without mapping to a domain capability.

## Pattern Shape

1. Keep domain capability in `VAA.EAPI.GraphQL.Domain.*`.
2. Register capability through domain `Startup : IStartup`.
3. Let host startup compose domain registrations.
4. Expose through GraphQL, REST router, and/or MCP using adapter layers.
5. Keep error mapping, auth visibility, telemetry, and health behavior consistent across hosts.

## Implementation Guidance

- Primary host should remain the canonical GraphQL execution surface.
- REST router should route to registered domain operations; it should not duplicate business orchestration.
- MCP handling should expose the same operation semantics as routed domain operations.
- Stitch host should compose remote schemas and middleware without embedding feature-specific orchestration.
- Keep warmup and heartbeat endpoints present and operational on each host.
- Keep host-level middleware focused on cross-cutting concerns: auth, validation, compression, telemetry, request shaping.

## Host Responsibilities

| Host | Responsibility |
| --- | --- |
| `VAA.EAPI.GraphQL` | GraphQL schema execution and domain startup composition for first-party domain operations. |
| `VAA.EAPI.GraphQL.Rest` | REST and MCP routing adapter over registered domain queries/mutations. |
| `VAA.EAPI.GraphQL.Stitch` | Stitched gateway over configured remote GraphQL endpoints. |

## Code References

- [Program.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL/Program.cs:1): primary GraphQL host bootstrap.
- [HostBuilderInitialiser.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL/FunctionHost/HostBuilderInitialiser.cs:1): primary host startup composition and domain discovery.
- [Program.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Rest/Program.cs:1): REST host bootstrap.
- [HostBuilderInitialiser.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Rest/FunctionHost/HostBuilderInitialiser.cs:1): REST host startup composition.
- [Router.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Rest/FunctionHost/Router.cs:28): route and MCP execution adapter.
- [Program.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Stitch/Program.cs:1): stitch host bootstrap and remote schema composition.
- [GraphQLFunction.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Stitch/GraphQLFunction.cs:1): stitch endpoint function.

## Related Principles And Standards

- [Architecture Principles](../principles/architecture-principles.md)
- [Integration Principles](../principles/integration-principles.md)
- [Coding Standards](../standards/coding-standards.md)
- [GraphQL Domain Startup Pack](platform-runtime-graphql-domain-startup-pack.md)

## AI Agent Checklist

- Is the capability registered in the owning domain startup?
- Is business logic in domain providers/services, not host/router entrypoints?
- Do GraphQL, REST-router, and MCP surfaces preserve the same core capability semantics?
- Are auth visibility and error-mapping behaviors consistent across surfaces?
- Are host operational endpoints and middleware behavior preserved?

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-25 | Added GraphQL host-topology guidance for primary host, REST router, and stitch runtime composition. | Dimitris Dimitriou |
