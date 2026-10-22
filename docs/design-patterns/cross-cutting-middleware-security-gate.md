# Middleware Security Gate

**Pattern group:** Cross-Cutting

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Apply authentication, authorization, exception handling, compression, and function context setup through EAPI middleware so HTTP-triggered functions have consistent platform security and operational behavior.

This pattern keeps security and operational policy out of individual handlers while allowing endpoints to declare role requirements.

## Use When

- Building or reviewing an HTTP-triggered EAPI Function app.
- Adding authentication or authorization to a service.
- Applying consistent exception and response behavior across functions.

## Do Not Use When

- The function is non-HTTP and does not pass through HTTP security policy.
- A service has an approved custom security boundary.
- Debug-only bypasses would leak into production behavior.

## Pattern Shape

1. Host registers default middleware.
2. Function context accessor initializes `AsyncContext`.
3. Authentication middleware validates bearer tokens against approved tenants.
4. Authorization middleware checks accepted app roles.
5. Exception middleware creates controlled responses for failures.

## Implementation Guidance

- Register middleware through the established `AddDefaultMiddleware()` extension where available.
- Use role attributes for endpoint-specific authorization.
- Do not implement custom token parsing in handlers.
- Keep Swagger exceptions explicit and safe.

## Code References

- [IFunctionsWorkerApplicationBuilderExtension.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Infrastructure/Extentions/IFunctionsWorkerApplicationBuilderExtension.cs:10): default middleware registration.
- [AuthenticationMiddleware.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Infrastructure/Middleware/AuthenticationMiddleware.cs:77): token validation and HTTP auth gate.
- [AuthorizationMiddleware.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Infrastructure/Middleware/AuthorizationMiddleware.cs:17): role-based authorization middleware.

## Related Principles And Standards

- [Security Principles](../principles/security-principles.md)
- [Infrastructure Principles](../principles/infrastructure-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- Default middleware is registered for HTTP services.
- Handlers do not perform ad hoc token parsing.
- Role requirements are declared where needed.
- Unauthorized and forbidden responses fail closed.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
