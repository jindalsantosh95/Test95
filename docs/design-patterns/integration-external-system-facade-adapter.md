# External System Facade Adapter

**Pattern group:** Integration

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Isolate partner and downstream system contracts behind facade handlers and infrastructure services that translate between EAPI request/response models and external API models.

This pattern protects EAPI callers from system-specific quirks, error formats, authentication flows, and payload structures.

## Use When

- Integrating with external or legacy systems such as PDM, IFG, Wallet, B2C, D365, AIR4, ITA, or Virgin Red.
- The downstream contract differs from EAPI's public API shape.
- Downstream errors need translation into EAPI errors.

## Do Not Use When

- The capability belongs inside an existing bounded-context microservice.
- The facade would simply proxy without adding contract isolation or policy.
- A direct GraphQL composition layer is the approved boundary.

## Pattern Shape

1. Accept the EAPI-facing command or request.
2. Validate and map into the downstream contract.
3. Acquire credentials or tokens as required.
4. Call the downstream service through connector-based infrastructure.
5. Map downstream response or errors into the EAPI envelope.

## Implementation Guidance

- Keep downstream DTOs and mappers separate from public EAPI DTOs.
- Keep external endpoint and application IDs in configuration.
- Do not leak downstream error bodies directly to callers.
- Prefer explicit mapping over generic object copying.

## Code References

- [RetroCreditCommandHandler.cs](../../VAA.EAPI.Facade/VAA.Facade.PDMFacade/Application/Command/RetroCreditCommandHandler.cs:17): PDM facade handler.
- [AuthorizeCommandHandler.cs](../../VAA.EAPI.Facade/VAA.Facade.NDC.IFGFacade/Application/Command/AuthorizeCommandHandler.cs:21): IFG/payment facade orchestration.
- [PaymentMSRefundCommandHandler.cs](../../VAA.EAPI.Facade/VAA.Facade.NDC.IFGFacade/Infrastructure/Services/PaymentMS/PaymentMSRefundCommandHandler.cs:15): downstream service adapter.

## Related Principles And Standards

- [Architecture Principles](../principles/architecture-principles.md)
- [Integration Principles](../principles/integration-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- Public EAPI models and downstream models remain separate.
- Downstream auth and endpoint settings come from configuration.
- Errors are mapped to EAPI error details.
- The facade adds meaningful isolation, policy, or translation.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
