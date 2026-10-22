# BFF Aggregation And Enrichment

**Pattern group:** GraphQL Composition

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Use GraphQL domain resolvers and services to compose multiple downstream capabilities into frontend-focused responses, enriching data with reference data, basket state, analytics context, and mapped downstream responses.

This pattern keeps orchestration in the GraphQL layer while leaving durable business ownership with the underlying services and bounded contexts.

## Use When

- A frontend journey needs data from multiple microservices or downstream systems.
- The response is tailored to a BFF-style customer or colleague experience.
- Reference data or cached lookup data is needed to shape the response.

## Do Not Use When

- The logic changes durable domain state that belongs in a microservice.
- GraphQL would become the owner of another service's data.
- The resolver would call downstream systems directly instead of using domain services.

## Pattern Shape

1. Resolver receives the GraphQL request and context.
2. Domain service coordinates downstream calls and cached reference lookups.
3. Mapper combines downstream responses into a BFF response model.
4. Errors are mapped into GraphQL/EAPI error conventions.

## Implementation Guidance

- Keep resolver methods thin.
- Put orchestration in domain services or base query classes.
- Use `IHttpConnector` through domain infrastructure services.
- Keep BFF response models separate from downstream DTOs.
- Preserve correlation and analytics context.

## Code References

- [OfferSearchBFFBase.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Domain.Search/Queries/OfferSearchBFFBase.cs:52): search aggregation and enrichment flow.
- [Startup.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Domain.Search/Startup.cs:35): services registered for BFF composition.
- [OfferQueryBFF.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Domain.Search/Queries/OfferQueryBFF.cs:14): GraphQL query entry point for BFF flow.

## Related Principles And Standards

- [Architecture Principles](../principles/architecture-principles.md)
- [Integration Principles](../principles/integration-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- GraphQL orchestrates but does not own another service's durable data.
- Resolver delegates to services.
- Enrichment uses approved domain services and cached reference services.
- Downstream DTOs are mapped before returning a BFF model.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
