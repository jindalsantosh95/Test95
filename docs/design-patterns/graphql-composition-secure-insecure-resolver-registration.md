# Secure/Insecure Resolver Registration

**Pattern group:** GraphQL Composition

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Register GraphQL query and mutation types conditionally based on EAPI visibility mode so secure operations and publicly visible operations can be composed in the same domain startup without exposing the wrong schema surface.

This pattern supports operational control of the GraphQL schema while preserving domain ownership of resolver registration.

## Use When

- Adding a GraphQL resolver that should only be exposed in a particular visibility mode.
- Splitting secure and non-secure schema capabilities.
- Reviewing GraphQL schema registration for exposure risk.

## Do Not Use When

- The resolver should always be available in every mode.
- Runtime authorization should be used instead of hiding the schema element.
- The capability belongs in a different API boundary.

## Pattern Shape

1. Host reads visibility mode from configuration.
2. Host passes `isSecure` into each domain startup registration method.
3. Domain startup conditionally registers query and mutation types.
4. Host adds root `Query` and `Mutation` types only when needed.

## Implementation Guidance

- Keep the visibility decision in startup registration, not inside resolver methods.
- Do not rely on visibility mode as the only authorization control for sensitive operations.
- Preserve debugger behavior only where already established and intentional.
- Review schema exposure when adding new resolver types.

## Code References

- [HostBuilderInitialiser.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL/FunctionHost/HostBuilderInitialiser.cs:136): reads operations visibility mode.
- [Startup.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Domain.Search/Startup.cs:68): conditional query registration.
- [Startup.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Domain.Book/Startup.cs:49): conditional mutation and query registration.

## Related Principles And Standards

- [Architecture Principles](../principles/architecture-principles.md)
- [Security Principles](../principles/security-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- Resolver exposure is registered in the owning domain startup.
- Secure visibility rules are checked before adding query or mutation types.
- Authorization still protects sensitive operations.
- Schema changes are intentional and reviewed.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
