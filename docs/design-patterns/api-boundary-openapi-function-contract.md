# OpenAPI Function Contract

**Pattern group:** API Boundary

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Make each HTTP-triggered Azure Function self-describing through OpenAPI attributes that define the operation, security scheme, request body, and response contract.

This pattern helps consumers, testers, gateway configuration, and AI agents understand the public contract without reverse-engineering function code.

## Use When

- Adding or changing an HTTP trigger.
- Publishing an endpoint through APIM or another API boundary.
- Updating request or response DTOs.

## Do Not Use When

- The trigger is not HTTP based.
- The endpoint is internal runtime plumbing and has no API contract.
- The OpenAPI metadata would describe behavior that the implementation does not actually provide.

## Pattern Shape

1. Decorate the function with `OpenApiOperation`.
2. Add the correct security scheme when the endpoint is protected.
3. Add request body documentation for body-based methods.
4. Add response documentation for success and expected error cases.
5. Keep the route and function name aligned with the API purpose.

## Implementation Guidance

- Use the existing EAPI OpenAPI helper types and security flow classes.
- Document `Unauthorized`, `Forbidden`, `BadRequest`, and `InternalServerError` where applicable.
- Keep request and response DTOs stable unless a breaking change is approved.
- Keep OpenAPI tags business-focused rather than implementation-focused.

## Code References

- [RefundsController.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Microservices.CRM.Refunds/Triggers/Http/RefundsController.cs:25): operation, OAuth, request body, and response metadata.
- [OpenApiAuth.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Infrastructure/Controllers/Extensions/OpenAPI/OpenApiAuth.cs:8): shared OAuth security flow.
- [LoungeEligibilityController.cs](../../VAA.EAPI.LoungeManagement/VAA.EAPI.LoungeManagement.Clues/Triggers/Http/LoungeEligibilityController.cs:37): richer response contract with validation and auth errors.

## Related Principles And Standards

- [Architecture Principles](../principles/architecture-principles.md)
- [Security Principles](../principles/security-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- OpenAPI metadata matches the implementation.
- Security metadata is present where required.
- Error responses use EAPI error DTOs.
- Route, function name, operation ID, and tags are clear.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
