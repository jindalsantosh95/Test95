# Validation To Mapping Pipeline

**Pattern group:** Integration

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Use a consistent pipeline that validates an incoming request, enriches it where needed, maps it to a downstream request, calls the downstream system, then maps the response back into an EAPI response.

This pattern keeps request rules visible and prevents downstream-specific assumptions from spreading into triggers or public DTOs.

## Use When

- A request must be validated before calling a downstream service.
- Downstream requests require reference data or context enrichment.
- Multiple downstream actions follow the same validate-map-call-map rhythm.

## Do Not Use When

- Validation is purely transport-level and should stay at the boundary.
- Mapping would hide a simple one-to-one contract with no transformation.
- The downstream call belongs behind an existing gateway pattern.

## Pattern Shape

1. Validate the EAPI request and collect `ErrorDetails`.
2. Return `Status.BadRequest` if validation fails.
3. Enrich with reference data or domain context when needed.
4. Map to downstream request.
5. Call the downstream adapter.
6. Map downstream response and errors back to EAPI response.

## Implementation Guidance

- Keep validation functions deterministic and side-effect free where possible.
- Return all meaningful validation errors together.
- Keep mappers explicit and close to the feature.
- Do not call downstream services until required validation passes.

## Code References

- [PaymentRequestValidations.cs](../../VAA.EAPI.Facade/VAA.Facade.NDC.IFGFacade/Validations/PaymentRequestValidations.cs:16): request validation returning EAPI errors.
- [AuthorizeCommandHandler.cs](../../VAA.EAPI.Facade/VAA.Facade.NDC.IFGFacade/Application/Command/AuthorizeCommandHandler.cs:21): validate, enrich, map, call, map response.
- [RefundCommandHandler.cs](../../VAA.EAPI.Facade/VAA.Facade.NDC.IFGFacade/Application/Command/RefundCommandHandler.cs:17): refund-specific validate and map flow.

## Related Principles And Standards

- [Data Principles](../principles/data-principles.md)
- [Integration Principles](../principles/integration-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- Validation happens before downstream side effects.
- Validation errors use EAPI `ErrorDetails`.
- Mappers are explicit and reviewable.
- Downstream models do not leak into public EAPI models.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
