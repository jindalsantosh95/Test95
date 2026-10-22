# Error Code Mapping Contract

**Pattern group:** API Boundary

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Map business, downstream, and technical failures into EAPI `ErrorDetails` using code-backed configuration, correlation IDs, and timestamps.

This pattern gives consumers consistent error payloads while still allowing individual services to map downstream-specific error codes into EAPI error semantics.

## Use When

- Returning validation, business, downstream, or technical errors.
- Translating third-party error codes into EAPI error codes.
- Adding a new error condition to an API or handler.

## Do Not Use When

- The error is internal-only and should be logged but not returned.
- A downstream raw error contains sensitive or customer data.
- A service has an approved alternative error contract.

## Pattern Shape

1. Identify the source error code or failure type.
2. Read the service or shared error-code configuration.
3. Map to `ErrorDetails` with EAPI code, message, source, developer message, correlation ID, and timestamp.
4. Return the mapped details through the EAPI response envelope.

## Implementation Guidance

- Keep error code JSON with the owning service or established shared location.
- Do not expose stack traces, secrets, tokens, or raw sensitive downstream payloads.
- Use technical error mappings when a business mapping cannot be found.
- Include correlation ID in developer-facing diagnostics.

## Code References

- [ErrorMapper.cs](../../VAA.EAPI.Web/VAA.EAPI.Web.Common/Helper/ErrorMapper.cs:8): maps business and technical exceptions from error configuration.
- [OrderConfirmationRequestHandler.cs](../../VAA.EAPI.Web/VAA.EAPI.Web.Book/Domain/Book/Handler/OrderConfirmationRequestHandler.cs:88): maps GraphQL/downstream errors into EAPI error details.
- [AuthorizeCommandHandler.cs](../../VAA.EAPI.Facade/VAA.Facade.NDC.IFGFacade/Application/Command/AuthorizeCommandHandler.cs:21): returns validation errors through the EAPI envelope.

## Related Principles And Standards

- [Security Principles](../principles/security-principles.md)
- [Integration Principles](../principles/integration-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- Error response uses `ErrorDetails`.
- Sensitive details are logged safely and not returned to callers.
- Correlation ID is preserved.
- New error codes are added to the correct error-code file where required.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
