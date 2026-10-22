# Token-Brokered Downstream Call

**Pattern group:** Integration

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Acquire the correct AAD application or delegated token through an EAPI token provider before calling a protected downstream API through `IHttpConnector`.

This pattern keeps downstream identity, token caching, on-behalf-of behavior, and bearer token propagation consistent across services.

## Use When

- Calling a protected downstream API that requires AAD or B2C-backed tokens.
- A service must support both application and delegated user contexts.
- Token acquisition should be cached and shared.

## Do Not Use When

- The downstream dependency uses a non-AAD authentication scheme with an approved client.
- The endpoint is unauthenticated and does not require an outbound token.
- The token would be manually assembled or hardcoded.

## Pattern Shape

1. Resolve downstream app ID, scope, authority, and credentials from configuration.
2. Use `IOAuthTokenProvider` to acquire an application or delegated token.
3. Pass the token value to `IHttpConnector`.
4. Let the connector write or overwrite the `Authorization` header.

## Implementation Guidance

- Never log token values.
- Cache tokens only through approved token-provider behavior.
- Use managed identity or approved secret storage for credentials.
- Preserve the inbound user context where on-behalf-of is required.

## Code References

- [OAuthTokenProvider.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Infrastructure/Services/OAuthTokenProvider.cs:22): application and delegated token acquisition.
- [RetroCreditCommandHandler.cs](../../VAA.EAPI.Facade/VAA.Facade.PDMFacade/Application/Command/RetroCreditCommandHandler.cs:28): token acquisition before downstream call.
- [HttpConnector.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Infrastructure/Services/HttpConnector.cs:50): connector bearer-token handling.

## Related Principles And Standards

- [Security Principles](../principles/security-principles.md)
- [Integration Principles](../principles/integration-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- Token provider is used instead of manual token construction.
- Token scope/app ID comes from configuration.
- Bearer token is passed to the connector, not added manually in feature code.
- Logs and errors do not expose token material.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
