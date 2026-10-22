# Connector-Based Downstream Gateway

**Pattern group:** Integration

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Route outbound HTTP calls from application and business code through EAPI connector abstractions and service-specific gateway classes.

This pattern centralises request construction, common headers, bearer-token handling, serialization, tracing, and error translation while keeping downstream-specific contracts behind an interface.

## Use When

- Calling a downstream HTTP API from an EAPI service, facade, or GraphQL domain.
- Creating a service-specific integration boundary such as D365, AIR4, ITA, PDM, IFG, Virgin Red, or Reference MS.
- Moving direct `HttpClient` usage out of feature code.

## Do Not Use When

- The integration is an approved SDK-style infrastructure client.
- The downstream call is not HTTP based.
- The code is test-only infrastructure.

## Pattern Shape

1. Define a gateway/service interface for the downstream capability.
2. Inject `IHttpConnector` into the gateway implementation.
3. Build the downstream URI and request-specific headers from configuration.
4. Pass bearer tokens through the connector.
5. Return a typed downstream response or mapped domain response.

## Implementation Guidance

- Do not create `HttpClient` in feature code.
- Let `HttpConnector` own common headers and bearer-token handling.
- Keep downstream DTOs inside infrastructure or gateway-specific folders.
- Do not log tokens or raw sensitive payloads.

## Code References

- [IHttpConnector.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Infrastructure/Interfaces/IHttpConnector.cs:8): connector abstraction.
- [HttpConnector.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Infrastructure/Services/HttpConnector.cs:42): shared request construction and bearer header handling.
- [D365RefundGateway.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Microservices.CRM.Refunds/Infrastructure/Services/D365RefundGateway.cs:7): downstream gateway boundary.

## Related Principles And Standards

- [Integration Principles](../principles/integration-principles.md)
- [Security Principles](../principles/security-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- Feature code depends on a gateway interface, not raw `HttpClient`.
- Gateway uses `IHttpConnector`.
- URI and tokens come from configuration or token providers.
- Sensitive request and response data is not logged.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
